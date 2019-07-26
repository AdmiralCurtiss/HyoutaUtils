using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;

namespace HyoutaUtils.Streams {
	public class ConcatenatedStream : DuplicatableStream {
		private List<DuplicatableStream> Streams;
		private List<long> Offsets; // contains summed length of the 0th to the nth streams
		private bool InternalCanRead;
		private bool InternalCanSeek;
		private bool InternalCanWrite;

		private int CurrentStreamIndex;

		private ConcatenatedStream( List<DuplicatableStream> streams, List<long> offsets, bool canRead, bool canSeek, bool canWrite ) {
			Streams = streams;
			Offsets = offsets;
			InternalCanRead = canRead;
			InternalCanSeek = canSeek;
			InternalCanWrite = canWrite;
			CurrentStreamIndex = 0;
		}

		public static DuplicatableStream CreateConcatenatedStream( List<DuplicatableStream> streams ) {
			if ( streams.Count == 0 ) {
				return EmptyStream.Instance;
			}

			var Streams = new List<DuplicatableStream>( streams.Count );
			var Offsets = new List<long>( Streams.Count );
			var InternalCanRead = true;
			var InternalCanSeek = true;
			var InternalCanWrite = true;
			long sum = 0;
			for ( int i = 0; i < streams.Count; ++i ) {
				DuplicatableStream s = streams[i].Duplicate();
				s.ReStart();
				long l = s.Length;
				if ( l <= 0 ) {
					s.End();
					s.Dispose();
					continue;
				}
				sum += l;
				InternalCanRead = InternalCanRead && s.CanRead;
				InternalCanSeek = InternalCanSeek && s.CanSeek;
				InternalCanWrite = InternalCanWrite && s.CanWrite;
				s.End();
				Streams.Add( s );
				Offsets.Add( sum );
			}

			if ( Streams.Count == 0 ) {
				return EmptyStream.Instance;
			}

			if ( Streams.Count == 1 ) {
				return Streams[0];
			}

			return new ConcatenatedStream( Streams, Offsets, InternalCanRead, InternalCanSeek, InternalCanWrite );
		}

		public override bool CanRead => InternalCanRead;
		public override bool CanSeek => InternalCanSeek;
		public override bool CanWrite => InternalCanWrite;
		public override long Length => Offsets[Offsets.Count - 1];

		private long BytesBeforeCurrentStream => CurrentStreamIndex == 0 ? 0 : Offsets[CurrentStreamIndex - 1];
		private DuplicatableStream CurrentStream => CurrentStreamIndex == Streams.Count ? EmptyStream.Instance : Streams[CurrentStreamIndex];

		public override long Position {
			get => BytesBeforeCurrentStream + CurrentStream.Position;
			set {
				if ( value < 0 ) {
					throw new Exception( "Invalid position for concatenated stream." );
				}

				// look through offsets and find the stream this position belongs to
				int index = 0;
				for ( int i = 0; i < Offsets.Count; ++i ) {
					if ( value < Offsets[i] ) {
						break;
					}
					++index;
				}

				// end currently active stream if it's a different one than the one we just determined
				if ( CurrentStreamIndex != index ) {
					CurrentStream.End();
				}

				if ( index < Offsets.Count ) {
					// position in a valid stream, set current stream and position within
					CurrentStreamIndex = index;
					CurrentStream.Position = value - BytesBeforeCurrentStream;
				} else {
					// position past the end
					CurrentStreamIndex = Offsets.Count;
				}
			}
		}

		public override void Flush() {
			CurrentStream.Flush();
		}

		public override int ReadByte() {
			int v = CurrentStream.ReadByte();
			if ( v > -1 && CurrentStream.Position >= CurrentStream.Length ) {
				CurrentStream.End();
				++CurrentStreamIndex;
			}
			return v;
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			if ( CurrentStreamIndex == Streams.Count ) {
				return 0;
			}

			int bytesLeftInCurrentStream = (int)( CurrentStream.Length - CurrentStream.Position );
			int bytesToReadFromCurrentStream = Math.Min( count, bytesLeftInCurrentStream );
			int bytesToReadFromNextStream = count - bytesToReadFromCurrentStream;
			int bytesRead = CurrentStream.Read( buffer, offset, bytesToReadFromCurrentStream );
			if ( bytesLeftInCurrentStream == bytesRead ) {
				CurrentStream.End();
				++CurrentStreamIndex;
			}
			if ( bytesToReadFromNextStream > 0 ) {
				return bytesRead + Read( buffer, offset + bytesRead, bytesToReadFromNextStream );
			} else {
				return bytesRead;
			}
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			switch ( origin ) {
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position = Position + offset;
					break;
				case SeekOrigin.End:
					Position = Length - offset; // FIXME: is that right?
					break;
			}
			return Position;
		}

		public override void SetLength( long value ) {
			throw new Exception( "Cannot set length of concatenated stream." );
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			if ( CurrentStreamIndex == Streams.Count ) {
				return;
			}

			int bytesLeftInCurrentStream = (int)( CurrentStream.Length - CurrentStream.Position );
			int bytesToWriteToCurrentStream = Math.Min( count, bytesLeftInCurrentStream );
			int bytesToWriteToNextStream = count - bytesToWriteToCurrentStream;
			CurrentStream.Write( buffer, offset, bytesToWriteToCurrentStream );
			if ( bytesLeftInCurrentStream == bytesToWriteToCurrentStream ) {
				CurrentStream.End();
				++CurrentStreamIndex;
			}
			if ( bytesToWriteToNextStream > 0 ) {
				Write( buffer, offset + bytesToWriteToCurrentStream, bytesToWriteToNextStream );
			}
		}

		public override DuplicatableStream Duplicate() {
			var streams = new List<DuplicatableStream>( Streams.Count );
			for ( int i = 0; i < Streams.Count; ++i ) {
				streams.Add( Streams[i].Duplicate() );
			}
			return new ConcatenatedStream( streams, Offsets, InternalCanRead, InternalCanSeek, InternalCanWrite );
		}

		public override void ReStart() {
			Position = 0;
		}

		public override void End() {
			CurrentStream.End();
			CurrentStreamIndex = 0;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append( "Chain of " ).Append( Streams.Count ).Append( " streams { " );
			for ( int i = 0; i < Streams.Count; ++i ) {
				if ( i != 0 ) {
					sb.Append( ", " );
				}
				sb.Append( Streams[i].ToString() );
			}
			sb.Append( " }" );
			return sb.ToString();
		}
	}
}
