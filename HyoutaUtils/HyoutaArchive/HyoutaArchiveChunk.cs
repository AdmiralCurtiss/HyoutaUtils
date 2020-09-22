using HyoutaPluginBase;
using HyoutaPluginBase.FileContainer;
using HyoutaUtils.Checksum;
using HyoutaUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;
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
				byte packedAlignment = (byte)(extra & 0x1fu);
				long unpackedAlignment = 1l << packedAlignment;
				bool hasMetadata = (extra & 0x20) != 0;
				bool isCompressed = (extra & 0x40) != 0;
				bool isBigEndian = (extra & 0x80) != 0;
				EndianUtils.Endianness e = isBigEndian ? EndianUtils.Endianness.BigEndian : EndianUtils.Endianness.LittleEndian;
				ulong endOfFileOffset = data.ReadUInt64(e) << packedAlignment;
				ulong tableOfContentsOffset = data.ReadUInt64(e) << packedAlignment;
				ulong filecount = data.ReadUInt64(e);
				chunkLength = endOfFileOffset;

				if (hasMetadata) {
					// just skip past this for now
					ulong metadataLength = data.ReadUInt64(e);
					data.DiscardBytes(metadataLength);
				}

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
						ulong filesize = dataBlockStream.ReadUInt64(e);
						HyoutaArchiveFileInfo fi = new HyoutaArchiveFileInfo();
						if (hasDummyContent) {
							fi.DummyContent = dataBlockStream.ReadBytes(dummyContentLength);
						}
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

		private static uint ReadContentLength(DuplicatableStream dataBlockStream, EndianUtils.Endianness e) {
			ushort l = dataBlockStream.ReadUInt16(e);
			return l == 0 ? 0x10000 : (uint)l;
		}

		private static void WriteContentLength(uint value, Stream stream, EndianUtils.Endianness e) {
			if (value == 0 || value > 0x10000) {
				throw new Exception("invalid content length");
			}
			stream.WriteUInt16((ushort)(value == 0x10000 ? 0 : value), e);
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

		private static byte[] EncodeString(string value, EndianUtils.Endianness e) {
			using (var ms = new MemoryStream()) {
				ms.WriteUInt64(0);
				ms.WriteString(Encoding.UTF8, value);
				ms.Position = 0;
				ms.WriteUInt64((ulong)ms.Length - 8, e);
				return ms.CopyToByteArrayAndDispose();
			}
		}

		public static void Pack(Stream target, List<HyoutaArchiveFileInfo> files, byte packedAlignmentRaw, EndianUtils.Endianness endian, HyoutaArchiveChunkInfo chunkInfo, HyoutaArchiveCompressionInfo compressionInfo) {
			long startPosition = target.Position;

			bool hasMetadata = chunkInfo != null;
			bool isCompressed = compressionInfo != null;
			byte packedAlignment = (byte)(packedAlignmentRaw & 0x1fu);
			byte extra = (byte)(packedAlignment | (hasMetadata ? 0x20 : 0) | (isCompressed ? 0x40 : 0) | (endian == EndianUtils.Endianness.BigEndian ? 0x80 : 0));
			target.WriteUInt64(0x6b6e7568636168ul | (((ulong)extra) << 56), EndianUtils.Endianness.LittleEndian);
			target.WriteUInt64(0); // aligned end-of-file offset, fill in later
			target.WriteUInt64(0); // aligned table of contents offset in data block, fill in later
			target.WriteUInt64((ulong)files.Count, endian);

			if (hasMetadata) {
				PackMetadata(target, chunkInfo, packedAlignment, endian, startPosition);
			}

			ulong tableOfContentsOffsetInDataBlock;
			if (isCompressed) {
				byte smallPackedAlignment = ToSmallPackedAlignment(packedAlignment);
				MemoryStream ms = new MemoryStream();
				tableOfContentsOffsetInDataBlock = PackDataBlock(ms, files, packedAlignment, endian);
				ms.Position = 0;
				(byte[] compressionInfoBytes, byte[] compressedData) = HyoutaArchiveCompression.Compress(compressionInfo, ms, packedAlignment);

				uint compressionInfoLength = (uint)compressionInfoBytes.Length.Align(1 << smallPackedAlignment);
				target.WriteUInt16((ushort)(compressionInfoLength == 0x10000 ? 0 : compressionInfoLength), endian);
				target.Write(compressionInfoBytes);
				target.WriteZeros(compressionInfoLength - compressionInfoBytes.Length);
				long dataBlockStartPos = (target.Position - startPosition).Align(1 << packedAlignment);
				target.WriteZeros(dataBlockStartPos - (target.Position - startPosition));
				target.Write(compressedData);
			} else {
				long dataBlockStartPos = (target.Position - startPosition).Align(1 << packedAlignment);
				target.WriteZeros(dataBlockStartPos - (target.Position - startPosition));
				tableOfContentsOffsetInDataBlock = PackDataBlock(target, files, packedAlignment, endian);
			}

			long endPosition = target.Position;
			long fileLength = endPosition - startPosition;
			target.Position = startPosition + 8;
			target.WriteUInt64(((ulong)fileLength) >> packedAlignment, endian);
			target.WriteUInt64(tableOfContentsOffsetInDataBlock >> packedAlignment, endian);
			target.Position = endPosition;
		}

		private static byte ToSmallPackedAlignment(byte packedAlignment) {
			// reduce the alignment to 16 if it's larger than that
			// this is used for the individual file info contents instead because otherwise a large alignment would waste a *ton* of bytes with no practical benefit
			// (as far as I know, anyway; you usually align either to disk sector sizes for loading efficiency, or processor word size, and the latter should be the only relevant one if align *within* a data chunk)
			return packedAlignment > 4 ? (byte)4 : packedAlignment;
		}

		private static void PackMetadata(Stream target, HyoutaArchiveChunkInfo chunkInfo, byte packedAlignment, EndianUtils.Endianness endian, long fileStartPosition) {
			// TODO: actually pack stuff here
			byte smallPackedAlignment = ToSmallPackedAlignment(packedAlignment);
			ulong length = 8; // length including the intial 8 byte length field, for data alignment
			target.WriteUInt64(length.Align(1 << smallPackedAlignment) - 8, endian);
			return;
		}

		private static ulong PackDataBlock(Stream target, List<HyoutaArchiveFileInfo> files, byte packedAlignment, EndianUtils.Endianness endian) {
			byte smallPackedAlignment = ToSmallPackedAlignment(packedAlignment);

			long startPosition = target.Position;
			target.WriteUInt16(0); // offsetToFirstFileInfo, fill in later

			bool hasDummyContent = files.Any(x => x.DummyContent != null);
			uint dummyContentLength = hasDummyContent ? ((uint)files.Max(x => x.DummyContent?.Length ?? 0)).Align(1 << smallPackedAlignment) : 0;
			bool hasFilename = files.Any(x => x.Filename != null);
			uint filenameLength = hasFilename ? ((uint)files.Max(x => x.Filename != null ? EncodeString(x.Filename, endian).Length : 0)).Align(1 << smallPackedAlignment) : 0;
			bool hasCompression = false; // TODO: implement this
			uint compressionInfoLength = 0; // TODO: implement this
			bool hasBpsPatch = false; // TODO: implement this
			uint bpsPatchInfoLength = 0; // TODO: implement this
			bool hasCrc32 = files.Any(x => x.crc32 != null);
			uint crc32ContentLength = hasCrc32 ? 4u.Align(1 << smallPackedAlignment) : 0u;
			bool hasMd5 = files.Any(x => x.md5 != null);
			uint md5ContentLength = hasMd5 ? 16u.Align(1 << smallPackedAlignment) : 0u;
			bool hasSha1 = files.Any(x => x.sha1 != null);
			uint sha1ContentLength = hasSha1 ? 20u.Align(1 << smallPackedAlignment) : 0u;

			ushort contentBitfield1 = 0;
			contentBitfield1 |= (ushort)(hasDummyContent ? 0x0001u : 0);
			contentBitfield1 |= (ushort)(hasFilename ? 0x0002u : 0);
			contentBitfield1 |= (ushort)(hasCompression ? 0x0004u : 0);
			contentBitfield1 |= (ushort)(hasBpsPatch ? 0x0008u : 0);
			contentBitfield1 |= (ushort)(hasCrc32 ? 0x0010u : 0);
			contentBitfield1 |= (ushort)(hasMd5 ? 0x0020u : 0);
			contentBitfield1 |= (ushort)(hasSha1 ? 0x0040u : 0);
			target.WriteUInt16(contentBitfield1, endian);

			if (hasDummyContent) {
				WriteContentLength(dummyContentLength, target, endian);
			}
			if (hasFilename) {
				WriteContentLength(filenameLength, target, endian);
			}
			if (hasCompression) {
				WriteContentLength(compressionInfoLength, target, endian);
			}
			if (hasBpsPatch) {
				WriteContentLength(bpsPatchInfoLength, target, endian);
			}
			if (hasCrc32) {
				WriteContentLength(crc32ContentLength, target, endian);
			}
			if (hasMd5) {
				WriteContentLength(md5ContentLength, target, endian);
			}
			if (hasSha1) {
				WriteContentLength(sha1ContentLength, target, endian);
			}

			long offsetToFirstFileInfo = (target.Position - startPosition).Align(1 << smallPackedAlignment);
			StreamUtils.WriteZeros(target, offsetToFirstFileInfo - (target.Position - startPosition));
			target.Position = startPosition;
			WriteContentLength((uint)offsetToFirstFileInfo, target, endian);
			target.Position = startPosition + offsetToFirstFileInfo;

			long singleFileInfoLength = 16 + dummyContentLength + filenameLength + compressionInfoLength + bpsPatchInfoLength + crc32ContentLength + md5ContentLength + sha1ContentLength;
			long totalFileInfoLength = singleFileInfoLength * files.Count;
			long offsetToFirstFile = (offsetToFirstFileInfo + totalFileInfoLength).Align(1 << packedAlignment);
			StreamUtils.WriteZeros(target, offsetToFirstFile - offsetToFirstFileInfo);

			long offsetToNextFile = offsetToFirstFile;
			for (int i = 0; i < files.Count; ++i) {
				HyoutaArchiveFileInfo fi = files[i];
				using (DuplicatableStream fs = fi.DataStream.Duplicate()) {
					// write file info
					target.Position = (singleFileInfoLength * i) + offsetToFirstFileInfo + startPosition;
					target.WriteUInt64(((ulong)offsetToNextFile) >> packedAlignment, endian);
					target.WriteUInt64((ulong)fs.Length, endian);
					if (hasDummyContent) {
						if (fi.DummyContent != null) {
							target.Write(fi.DummyContent);
							target.WriteZeros(dummyContentLength - fi.DummyContent.Length);
						} else {
							target.WriteZeros(dummyContentLength);
						}
					}
					if (hasFilename) {
						if (fi.Filename != null) {
							byte[] str = EncodeString(fi.Filename, endian);
							target.Write(str);
							target.WriteZeros(filenameLength - str.Length);
						} else {
							target.WriteZeros(filenameLength);
						}
					}
					if (hasCompression) {
						// TODO
					}
					if (hasBpsPatch) {
						// TODO
					}
					if (hasCrc32) {
						if (fi.crc32.HasValue) {
							target.Write(fi.crc32.Value.Bytes);
							target.WriteZeros(crc32ContentLength - 4);
						} else {
							target.WriteZeros(crc32ContentLength);
						}
					}
					if (hasMd5) {
						if (fi.md5.HasValue) {
							target.Write(fi.md5.Value.Value);
							target.WriteZeros(md5ContentLength - 16);
						} else {
							target.WriteZeros(md5ContentLength);
						}
					}
					if (hasSha1) {
						if (fi.sha1.HasValue) {
							target.Write(fi.sha1.Value.Value);
							target.WriteZeros(sha1ContentLength - 20);
						} else {
							target.WriteZeros(sha1ContentLength);
						}
					}

					// write file and update next file offset
					target.Position = offsetToNextFile + startPosition;
					fs.Position = 0;
					StreamUtils.CopyStream(fs, target);
					long currentEnd = offsetToNextFile + fs.Length;
					offsetToNextFile = (target.Position - startPosition).Align(1 << packedAlignment);
					long filerest = offsetToNextFile - currentEnd;
					target.WriteZeros(filerest);
				}
			}

			long endPosition = offsetToNextFile + startPosition;
			target.Position = endPosition;

			return 0; // we currently pack the table of contents always at 0 in the data block; return offset here otherwise
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
