using HyoutaPluginBase;
using HyoutaPluginBase.FileContainer;
using HyoutaUtils.Checksum;
using HyoutaUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveChunk : IContainer {
		private List<HyoutaArchiveFileInfo> Files;

		public long Filecount => Files.Count;

		public bool IsFile => false;
		public bool IsContainer => true;
		public IFile AsFile => null;
		public IContainer AsContainer => this;

		public HyoutaArchiveChunk(DuplicatableStream duplicatableStream, out ulong chunkLength) {
			using (DuplicatableStream data = duplicatableStream.Duplicate()) {
				data.Position = 0;

				// header
				ulong extraMagic = data.ReadUInt64(EndianUtils.Endianness.LittleEndian);
				ulong magic = extraMagic & 0x00fffffffffffffful;
				if (magic != 0x6b6e7568636168) {
					throw new Exception("wrong magic");
				}
				byte extra = (byte)((extraMagic >> 56) & 0xffu);
				byte packedAlignment = (byte)(extra & 0x3fu);
				long unpackedAlignment = 1l << packedAlignment;
				bool isBigEndian = (extra & 0x40) != 0;
				bool isCompressed = (extra & 0x80) != 0;
				EndianUtils.Endianness e = isBigEndian ? EndianUtils.Endianness.BigEndian : EndianUtils.Endianness.LittleEndian;
				ulong endOfFileOffset = data.ReadUInt64(e) << packedAlignment;
				ulong tableOfContentsOffset = data.ReadUInt64(e) << packedAlignment;
				ulong filecount = data.ReadUInt64(e);
				chunkLength = endOfFileOffset;

				DuplicatableStream dataBlockStream;
				if (isCompressed) {
					ushort compressionInfoLengthRaw = data.ReadUInt16(e);
					uint compressionInfoLength = compressionInfoLengthRaw & 0xfffcu;
					int compressionInfoAlignmentPacked = (compressionInfoLengthRaw & 0x3) + 1;
					data.ReadAlign(1u << compressionInfoAlignmentPacked);
					HyoutaArchiveCompressionInfo compressionInfo = new HyoutaArchiveCompressionInfo(data, compressionInfoLength == 0 ? 0x10000u : compressionInfoLength);
					dataBlockStream = HyoutaArchiveDecompression.Decompress(compressionInfo, data);
				} else {
					data.ReadAlign(unpackedAlignment);
					dataBlockStream = new PartialStream(data, data.Position, (long)(endOfFileOffset - (ulong)data.Position));
				}

				try {
					data.Dispose();

					dataBlockStream.Position = (long)tableOfContentsOffset;
					uint offsetToFirstFileInfo = ReadContentLength(dataBlockStream, e);

					// decode content bitfield(s)
					long numberOfUnknownBits = 0;
					ushort contentBitfield1 = dataBlockStream.ReadUInt16(e);
					bool hasDummyContent = (contentBitfield1 & 0x0001u) != 0;
					bool hasFilename = (contentBitfield1 & 0x0002u) != 0;
					bool hasCompression = (contentBitfield1 & 0x0004u) != 0;
					bool hasBpsPatch = (contentBitfield1 & 0x0008u) != 0;
					bool hasCrc32 = (contentBitfield1 & 0x0010u) != 0;
					bool hasMd5 = (contentBitfield1 & 0x0020u) != 0;
					bool hasSha1 = (contentBitfield1 & 0x0040u) != 0;
					numberOfUnknownBits += (contentBitfield1 & 0x0080u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x0100u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x0200u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x0400u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x0800u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x1000u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x2000u) != 0 ? 1 : 0;
					numberOfUnknownBits += (contentBitfield1 & 0x4000u) != 0 ? 1 : 0;
					ushort currentBitfield = contentBitfield1;
					while ((currentBitfield & 0x8000u) != 0) {
						// more bitfields, though we don't understand them since only the first handful of bits are defined at the moment, so just count and skip them
						currentBitfield = dataBlockStream.ReadUInt16(e);
						numberOfUnknownBits += (currentBitfield & 0x0001u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0002u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0004u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0008u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0010u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0020u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0040u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0080u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0100u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0200u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0400u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x0800u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x1000u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x2000u) != 0 ? 1 : 0;
						numberOfUnknownBits += (currentBitfield & 0x4000u) != 0 ? 1 : 0;
					}
					uint dummyContentLength = hasDummyContent ? ReadContentLength(dataBlockStream, e) : 0;
					uint filenameLength = hasFilename ? ReadContentLength(dataBlockStream, e) : 0;
					uint compressionLength = hasCompression ? ReadContentLength(dataBlockStream, e) : 0;
					uint bpspatchLength = hasBpsPatch ? ReadContentLength(dataBlockStream, e) : 0;
					uint crc32Length = hasCrc32 ? ReadContentLength(dataBlockStream, e) : 0;
					uint md5Length = hasMd5 ? ReadContentLength(dataBlockStream, e) : 0;
					uint sha1Length = hasSha1 ? ReadContentLength(dataBlockStream, e) : 0;
					long unknownContentLength = 0;
					for (long i = 0; i < numberOfUnknownBits; ++i) {
						unknownContentLength += ReadContentLength(dataBlockStream, e);
					}

					dataBlockStream.Position = (long)(tableOfContentsOffset + offsetToFirstFileInfo);
					List<HyoutaArchiveFileInfo> files = new List<HyoutaArchiveFileInfo>((int)filecount);
					for (ulong i = 0; i < filecount; ++i) {
						ulong offset = dataBlockStream.ReadUInt64(e) << packedAlignment;
						ulong filesize = dataBlockStream.ReadUInt64(e) << packedAlignment;
						HyoutaArchiveFileInfo fi = new HyoutaArchiveFileInfo();
						dataBlockStream.DiscardBytes(dummyContentLength);
						if (hasFilename) {
							fi.Filename = ReadString(dataBlockStream, filenameLength, e);
						}
						if (hasCompression) {
							fi.Compression = new HyoutaArchiveCompressionInfo(dataBlockStream, compressionLength);
						}
						if (hasBpsPatch) {
							fi.BpsPatch = new HyoutaArchiveBpsPatchInfo(dataBlockStream, bpspatchLength);
						}
						if (hasCrc32) {
							if (crc32Length >= 4) {
								fi.crc32 = new CRC32(dataBlockStream.ReadUInt32(EndianUtils.Endianness.BigEndian));
								dataBlockStream.DiscardBytes(crc32Length - 4);
							} else {
								dataBlockStream.DiscardBytes(crc32Length);
							}
						}
						if (hasMd5) {
							if (md5Length >= 16) {
								ulong a = dataBlockStream.ReadUInt64(EndianUtils.Endianness.BigEndian);
								ulong b = dataBlockStream.ReadUInt64(EndianUtils.Endianness.BigEndian);
								fi.md5 = new MD5(a, b);
								dataBlockStream.DiscardBytes(md5Length - 16);
							} else {
								dataBlockStream.DiscardBytes(md5Length);
							}
						}
						if (hasSha1) {
							if (sha1Length >= 20) {
								ulong a = dataBlockStream.ReadUInt64(EndianUtils.Endianness.BigEndian);
								ulong b = dataBlockStream.ReadUInt64(EndianUtils.Endianness.BigEndian);
								uint c = dataBlockStream.ReadUInt32(EndianUtils.Endianness.BigEndian);
								fi.sha1 = new SHA1(a, b, c);
								dataBlockStream.DiscardBytes(sha1Length - 20);
							} else {
								dataBlockStream.DiscardBytes(sha1Length);
							}
						}
						dataBlockStream.DiscardBytes(unknownContentLength);

						fi.Data = new PartialStream(dataBlockStream, (long)offset, (long)filesize);
						files.Add(fi);
					}

					Files = files;
				} finally {
					dataBlockStream.Dispose();
				}
			}
		}

		private uint ReadContentLength(DuplicatableStream dataBlockStream, EndianUtils.Endianness e) {
			ushort l = dataBlockStream.ReadUInt16(e);
			return l == 0 ? 0x10000 : (uint)l;
		}

		private static string ReadString(DuplicatableStream s, uint maxBytes, EndianUtils.Endianness e) {
			if (maxBytes < 8) {
				// can't be a valid string
				s.DiscardBytes(maxBytes);
				return null;
			}

			ulong length = s.ReadUInt64(e);
			uint restBytes = maxBytes - 8;
			if (length > restBytes) {
				// can't be a valid string
				s.DiscardBytes(restBytes);
				return null;
			}

			string str = s.ReadSizedString((long)length, TextUtils.GameTextEncoding.UTF8);
			s.DiscardBytes(restBytes - length);
			return str;
		}

		public HyoutaArchiveFileInfo GetFile(long index) {
			return Files[(int)index];
		}

		public INode GetChildByIndex(long index) {
			return GetFile(index);
		}

		public INode GetChildByName(string name) {
			for (int i = 0; i < Files.Count; ++i) {
				if (Files[i].Filename == name) {
					return GetChildByIndex(i);
				}
			}
			return null;
		}

		public IEnumerable<string> GetChildNames() {
			for (int i = 0; i < Files.Count; ++i) {
				if (Files[i].Filename != null) {
					yield return Files[i].Filename;
				}
			}
		}

		private bool disposedValue;
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					for (int i = 0; i < Files.Count; ++i) {
						if (Files[i].Data != null) {
							Files[i].Data.Dispose();
						}
					}
				}
				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
