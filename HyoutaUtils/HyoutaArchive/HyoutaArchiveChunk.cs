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
					Compression.IHyoutaArchiveCompressionInfo compressionInfo = HyoutaArchiveCompression.Deserialize(data, compressionInfoLength == 0 ? 0x10000u : compressionInfoLength, e);
					dataBlockStream = compressionInfo.Decompress(data);
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
							fi.CompressionInfo = HyoutaArchiveCompression.Deserialize(dataBlockStream, compressionLength, e);
							fi.StreamIsCompressed = true;
						}
						if (hasBpsPatch) {
							fi.BpsPatchInfo = HyoutaArchiveBpsPatchInfo.Deserialize(dataBlockStream, bpspatchLength, e, i, this);
							fi.StreamIsBpsPatch = fi.BpsPatchInfo != null;
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

			ulong rawlength = s.ReadUInt64(e);
			ulong length = (rawlength & 0x7ffffffffffffffful);
			bool hasOffset = (rawlength & 0x8000000000000000ul) > 0;

			if (hasOffset) {
				// format is 8 bytes length, then 8 bytes position of string in data
				if (maxBytes < 16) {
					// can't be valid
					s.DiscardBytes(maxBytes - 8);
					return null;
				}

				ulong offset = s.ReadUInt64(e);
				long p = s.Position + (maxBytes - 16);
				s.Position = (long)offset;
				string str = s.ReadSizedString((long)length, TextUtils.GameTextEncoding.UTF8);
				s.Position = p;
				return str;
			} else {
				// format is 8 bytes length, then [number read] bytes string
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
		}

		private static void WriteString(Stream target, byte[] encodedString, uint maxBytes, EndianUtils.Endianness endian, long startPosition, ref long positionOfFreeSpace) {
			if (maxBytes < 8) {
				// not enough space to write string
				throw new Exception("invalid string length");
			}

			uint rest = maxBytes - 8;
			ulong length = (ulong)encodedString.LongLength;
			if (rest < 8) {
				// *must* write string in-place, since there's not enough space for the long one
				if (length > rest) {
					// not enough space to write string
					throw new Exception("invalid string length");
				}

				target.WriteUInt64(length, endian);
				target.Write(encodedString);
				target.WriteZeros(rest - encodedString.Length);
				return;
			}

			if (length > rest) {
				// we don't have enough space to write in-place, so we write an offset to elsewhere instead and write the string there
				target.WriteUInt64(length | 0x8000000000000000ul, endian);
				target.WriteUInt64((ulong)positionOfFreeSpace, endian);
				target.WriteZeros(maxBytes - 16);
				long p = target.Position;
				target.Position = startPosition + positionOfFreeSpace;
				target.Write(encodedString);
				positionOfFreeSpace = target.Position - startPosition;
				target.Position = p;
				return;
			}

			// otherwise write string in-place
			target.WriteUInt64(length, endian);
			target.Write(encodedString);
			target.WriteZeros(rest - encodedString.Length);
		}

		private static byte[] EncodeString(string value) {
			return Encoding.UTF8.GetBytes(value);
		}

		public static void Pack(Stream target, List<HyoutaArchiveFileInfo> files, byte packedAlignmentRaw, EndianUtils.Endianness endian, HyoutaArchiveChunkInfo chunkInfo, Compression.IHyoutaArchiveCompressionInfo compressionInfo) {
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
				(byte[] compressionInfoBytes, byte[] compressedData) = compressionInfo.Compress(ms, endian);

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
			uint filenameLength = 0;
			bool embedFilenamesInFileInfo = false;
			List<byte[]> encodedFilenames = null;
			if (hasFilename) {
				// figure out whether we want the strings to embed into the fileinfo directly
				// or whether to use an offset and write the string data at the end of the fileinfo
				// note that if a string is <= 8 bytes we can always embed it as we'd need 8 bytes for the offset anyway
				// so...
				encodedFilenames = new List<byte[]>(files.Count);
				long longestBytecount = 0;
				long totalBytecount = 0;
				long filenameCountOver8Bytes = 0;
				for (int i = 0; i < files.Count; ++i) {
					if (files[i].Filename == null) {
						encodedFilenames.Add(null);
					} else {
						byte[] stringbytes = EncodeString(files[i].Filename);
						encodedFilenames.Add(stringbytes);

						if (stringbytes.LongLength > 8) {
							longestBytecount = Math.Max(longestBytecount, stringbytes.LongLength);
							totalBytecount += stringbytes.LongLength;
							++filenameCountOver8Bytes;
						}
					}
				}

				// alright so we have, in practice, two options here
				// - make filenameLength == 16, store strings that are longer than that offsetted
				long nonEmbedSize = files.Count * 16 + totalBytecount.Align(1 << smallPackedAlignment);
				// - make filenameLength long enough so all strings can be embedded
				long embedSize = files.Count * (8 + longestBytecount).Align(1 << smallPackedAlignment);

				// pick whatever results in a smaller file; on a tie embed
				if (nonEmbedSize < embedSize) {
					embedFilenamesInFileInfo = false;
					filenameLength = 16;
				} else {
					embedFilenamesInFileInfo = true;
					filenameLength = (uint)(8 + longestBytecount).Align(1 << smallPackedAlignment);
				}
			}
			bool hasCompression = files.Any(x => x.CompressionInfo != null);
			uint compressionInfoLength = hasCompression ? files.Max(x => x.CompressionInfo?.MaximumCompressionInfoLength() ?? 0).Align(1 << smallPackedAlignment) : 0;
			bool hasBpsPatch = files.Any(x => x.BpsPatchInfo != null);
			uint bpsPatchInfoLength = hasBpsPatch ? 16u.Align(1 << smallPackedAlignment) : 0;
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
			long offsetToEndOfFileInfo = (offsetToFirstFileInfo + totalFileInfoLength).Align(1 << smallPackedAlignment);
			StreamUtils.WriteZeros(target, offsetToEndOfFileInfo - offsetToFirstFileInfo);

			var filedata = new List<(long position, DuplicatableStream data)>(files.Count);

			long positionOfFreeSpace = offsetToEndOfFileInfo;
			for (int i = 0; i < files.Count; ++i) {
				HyoutaArchiveFileInfo fi = files[i];
				using (DuplicatableStream fs = fi.Data.Duplicate()) {
					DuplicatableStream streamToWrite = fs;

					bool streamIsInternallyCompressed = fi.StreamIsCompressed;
					if (fi.BpsPatchInfo != null && fi.CompressionInfo != null && streamIsInternallyCompressed && !fi.StreamIsBpsPatch) {
						// this is a weird case; the stream wants both bps patch and compression
						// and is already compressed but not already bps patched, which breaks the defined order
						// we can handle this by decompressing, creating patch, recompressing
						streamToWrite = fi.DataStream.Duplicate(); // this decompresses the stream
						streamIsInternallyCompressed = false; // and fake-set the stream as uncompressed for packing logic
					}

					byte[] bpsPatchInfoBytes = null;
					byte[] compressionInfoBytes = null;
					if (hasBpsPatch) {
						if (fi.BpsPatchInfo == null) {
							// chunk has patches but this file is unpatched; we store this by pointing the file to itself
							bpsPatchInfoBytes = new HyoutaArchiveBpsPatchInfo((ulong)i, (ulong)streamToWrite.Length, null).Serialize(endian);
						} else if (fi.StreamIsBpsPatch) {
							bpsPatchInfoBytes = fi.BpsPatchInfo.Serialize(endian);
						} else {
							var p = HyoutaArchiveBps.CreatePatch(fi.BpsPatchInfo, streamToWrite, endian);
							bpsPatchInfoBytes = p.patchInfo;
							streamToWrite = new DuplicatableByteArrayStream(p.patchData);
						}
					}
					if (hasCompression && fi.CompressionInfo != null) {
						if (streamIsInternallyCompressed) {
							compressionInfoBytes = fi.CompressionInfo.Serialize(endian);
						} else {
							var p = fi.CompressionInfo.Compress(streamToWrite, endian);
							compressionInfoBytes = p.compressionInfo;
							streamToWrite = new DuplicatableByteArrayStream(p.compressedData);
						}
					}

					// write file info
					target.Position = (singleFileInfoLength * i) + offsetToFirstFileInfo + startPosition;
					long positionPosition = target.Position;
					target.WriteUInt64(0); // position of file, will be filled later
					target.WriteUInt64((ulong)streamToWrite.Length, endian);
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
							WriteString(target, encodedFilenames[i], filenameLength, endian, startPosition, ref positionOfFreeSpace);
						} else {
							target.WriteZeros(filenameLength);
						}
					}
					if (hasCompression) {
						if (compressionInfoBytes != null) {
							if (compressionInfoBytes.Length > compressionInfoLength) {
								throw new Exception("compression info too long");
							}
							target.Write(compressionInfoBytes);
							target.WriteZeros(compressionInfoLength - compressionInfoBytes.Length);
						} else {
							target.WriteZeros(compressionInfoLength);
						}
					}
					if (hasBpsPatch) {
						if (bpsPatchInfoBytes != null) {
							if (bpsPatchInfoBytes.Length > bpsPatchInfoLength) {
								throw new Exception("bps info too long");
							}
							target.Write(bpsPatchInfoBytes);
							target.WriteZeros(bpsPatchInfoLength - bpsPatchInfoBytes.Length);
						} else {
							target.WriteZeros(bpsPatchInfoLength);
						}
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

					filedata.Add((positionPosition, streamToWrite.Duplicate()));
				}
			}

			for (int i = 0; i < files.Count; ++i) {
				using (DuplicatableStream streamToWrite = filedata[i].data.Duplicate()) {
					// fill with zero until start of file
					long startOfFiledata = positionOfFreeSpace.Align(1 << packedAlignment);
					target.Position = startPosition + positionOfFreeSpace;
					target.WriteZeros(startOfFiledata - positionOfFreeSpace);

					// write file
					streamToWrite.Position = 0;
					StreamUtils.CopyStream(streamToWrite, target);
					positionOfFreeSpace = target.Position - startPosition;

					// write position of file
					target.Position = filedata[i].position;
					target.WriteUInt64(((ulong)(startOfFiledata)) >> packedAlignment, endian);
				}
			}

			// zero-align to end of file
			long endOfData = positionOfFreeSpace.Align(1 << packedAlignment);
			target.Position = startPosition + positionOfFreeSpace;
			target.WriteZeros(endOfData - positionOfFreeSpace);

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
