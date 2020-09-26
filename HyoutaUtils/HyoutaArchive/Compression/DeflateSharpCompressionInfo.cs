using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive.Compression {
	// using the built-in C# DeflateStream... this should be compatible with other deflate algorithms, but note it as a specific one just in case
	public class DeflateSharpCompressionInfo : IHyoutaArchiveCompressionInfo {
		public const ulong Identifier = 0x6465666C61746523u;
		public ulong UncompressedFilesize;

		public DeflateSharpCompressionInfo(ulong uncompressedFilesize) {
			UncompressedFilesize = uncompressedFilesize;
		}

		public (byte[] compressionInfo, byte[] compressedData) Compress(Stream data, EndianUtils.Endianness endian) {
			UncompressedFilesize = (ulong)data.Length;
			using (MemoryStream ms = new MemoryStream())
			using (DeflateStream compressed = new DeflateStream(ms, CompressionMode.Compress, true)) {
				data.Position = 0;
				StreamUtils.CopyStream(data, compressed);
				compressed.Close();
				return (Serialize(endian), ms.CopyToByteArray());
			}
		}

		public DuplicatableStream Decompress(Stream data) {
			using (MemoryStream ms = new MemoryStream())
			using (DeflateStream decompressed = new DeflateStream(data, CompressionMode.Decompress)) {
				StreamUtils.CopyStream(decompressed, ms, (long)UncompressedFilesize);
				return ms.CopyToByteArrayStreamAndDispose();
			}
		}

		public byte[] Serialize(EndianUtils.Endianness endian) {
			using (MemoryStream ms = new MemoryStream()) {
				ms.WriteUInt64(Identifier, EndianUtils.Endianness.BigEndian);
				ms.WriteUInt64(UncompressedFilesize, endian);
				return ms.CopyToByteArrayAndDispose();
			}
		}

		public static DeflateSharpCompressionInfo Deserialize(DuplicatableStream stream, long maxBytes, EndianUtils.Endianness endian) {
			// note: identifier has already been read
			if (maxBytes < 8) {
				stream.DiscardBytes(maxBytes);
				return null;
			}

			ulong uncompressedFilesize = stream.ReadUInt64(endian);
			stream.DiscardBytes(maxBytes - 8);
			return new DeflateSharpCompressionInfo(uncompressedFilesize);
		}
	}
}
