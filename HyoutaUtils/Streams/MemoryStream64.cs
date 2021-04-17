using System;
using System.IO;

namespace HyoutaUtils.Streams {
	public class MemoryStream64 : Stream {
		private class ByteArray64 {
			byte[][] buffer;
			public long Length { get; private set; }

			public ByteArray64(long length) {
				long outerIndex = length >> 30;
				long innerIndex = length & 0x3fffffffL;
				if (innerIndex > 0) {
					buffer = new byte[outerIndex + 1][];
					for (long i = 0; i < outerIndex; ++i) {
						buffer[i] = new byte[0x40000000];
					}
					buffer[outerIndex] = new byte[innerIndex];
				} else {
					buffer = new byte[outerIndex][];
					for (long i = 0; i < outerIndex; ++i) {
						buffer[i] = new byte[0x40000000];
					}
				}
				Length = length;
			}

			public byte Get(long i) {
				long outerIndex = i >> 30;
				long innerIndex = i & 0x3fffffffL;
				return buffer[outerIndex][innerIndex];
			}

			public void Set(long i, byte v) {
				long outerIndex = i >> 30;
				long innerIndex = i & 0x3fffffffL;
				buffer[outerIndex][innerIndex] = v;
			}

			public void Get(byte[] target, int targetIndex, long sourceIndex, int length) {
				long outerIndex = sourceIndex >> 30;
				long innerIndex = sourceIndex & 0x3fffffffL;
				long innerEnd = innerIndex + length;
				if (innerEnd > 0x40000000) {
					int firstBlockBytes = (int)(0x40000000 - innerIndex);
					int secondBlockBytes = length - firstBlockBytes;
					Buffer.BlockCopy(buffer[outerIndex], (int)innerIndex, target, targetIndex, firstBlockBytes);
					Buffer.BlockCopy(buffer[outerIndex + 1], 0, target, targetIndex + firstBlockBytes, secondBlockBytes);
				} else {
					Buffer.BlockCopy(buffer[outerIndex], (int)innerIndex, target, targetIndex, length);
				}
			}

			public void Set(long targetIndex, byte[] source, int sourceIndex, int length) {
				long outerIndex = targetIndex >> 30;
				long innerIndex = targetIndex & 0x3fffffffL;
				long innerEnd = innerIndex + length;
				if (innerEnd > 0x40000000) {
					int firstBlockBytes = (int)(0x40000000 - innerIndex);
					int secondBlockBytes = length - firstBlockBytes;
					Buffer.BlockCopy(source, sourceIndex, buffer[outerIndex], (int)innerIndex, firstBlockBytes);
					Buffer.BlockCopy(source, sourceIndex + firstBlockBytes, buffer[outerIndex + 1], 0, secondBlockBytes);
				} else {
					Buffer.BlockCopy(source, sourceIndex, buffer[outerIndex], (int)innerIndex, length);
				}
			}

			internal void AcquireFrom(ByteArray64 other) {
				for (long i = 0; i < other.buffer.LongLength; ++i) {
					Set(i << 30, other.buffer[i], 0, other.buffer[i].Length);
				}
			}

			public void SetZero(long targetIndex, long length) {
				// this can be optimized...
				for (long i = 0; i < length; ++i) {
					Set(i + targetIndex, 0);
				}
			}
		}

		private ByteArray64 _Buffer;
		private long _Length;
		private long _Position;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length => _Length;

		public override long Position {
			get => _Position; set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException();
				}
				_Position = value;
			}
		}

		public MemoryStream64(long initialCapacity = 0) {
			_Buffer = new ByteArray64(initialCapacity);
			_Length = 0;
			_Position = 0;
		}

		public override void Flush() { }

		public override int Read(byte[] buffer, int offset, int count) {
			if (count <= 0) {
				return 0;
			}

			long pos = _Position;
			long len = _Length;
			ByteArray64 buf = _Buffer;
			ulong endPositionUL = ((ulong)pos) + ((ulong)count);
			long endPosition;
			if (endPositionUL > (ulong)len) {
				endPosition = len;
			} else {
				endPosition = (long)endPositionUL;
			}
			int bytesToRead = (int)(endPosition - pos);
			buf.Get(buffer, offset, pos, bytesToRead);
			_Position = endPosition;
			return bytesToRead;
		}

		public override long Seek(long offset, SeekOrigin origin) {
			long pos;
			switch (origin) {
				case SeekOrigin.Begin: pos = offset; break;
				case SeekOrigin.Current: pos = _Position + offset; break;
				case SeekOrigin.End: pos = _Length - offset; break;
				default: throw new ArgumentException();
			}
			if (pos < 0) {
				throw new IOException();
			}
			_Position = pos;
			return pos;
		}

		public override void SetLength(long newLength) {
			if (newLength < 0) {
				throw new ArgumentOutOfRangeException();
			}
			Reserve(newLength);
			long oldLength = _Length;
			_Length = newLength;
			if (oldLength < newLength) {
				ByteArray64 buf = _Buffer;
				buf.SetZero(oldLength, newLength - oldLength);
			}
		}

		public override void Write(byte[] buffer, int offset, int count) {
			if (count <= 0) {
				return;
			}

			long pos = _Position;
			long len = _Length;
			ulong endPositionUL = ((ulong)pos) + ((ulong)count);
			long endPosition;
			if (endPositionUL > (ulong)len) {
				if (endPositionUL > (ulong)long.MaxValue) {
					throw new IOException();
				}
				Autogrow(endPositionUL);
				endPosition = (long)endPositionUL;
			} else {
				endPosition = (long)endPositionUL;
			}
			int bytesToWrite = count;
			ByteArray64 buf = _Buffer;
			buf.Set(pos, buffer, offset, bytesToWrite);
			_Position = endPosition;
			if (endPosition > len) {
				_Length = endPosition;
			}
		}

		private void Autogrow(ulong atLeast) {
			ByteArray64 buf = _Buffer;
			ulong len = (ulong)buf.Length;
			if (len < atLeast) {
				if (atLeast <= 0x1000 && len < 0x800) {
					Reserve(0x1000);
				} else {
					ulong capacity = Math.Max(atLeast, len + (len >> 1));
					if (capacity > (ulong)long.MaxValue) {
						// not like this will ever work on a 64 bit system...
						Reserve(long.MaxValue);
					} else {
						Reserve((long)capacity);
					}
				}
			}
		}

		public void Reserve(long capacity) {
			ByteArray64 buf = _Buffer;
			if (buf.Length < capacity) {
				ByteArray64 newBuf = new ByteArray64(capacity);
				long oldlen = buf.Length;
				newBuf.AcquireFrom(buf);
				_Buffer = newBuf;
			}
		}
	}
}
