using HyoutaPluginBase;
using HyoutaUtils.HyoutaArchive.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveCompression {
		public static Compression.IHyoutaArchiveCompressionInfo Deserialize(DuplicatableStream stream, long maxBytes, EndianUtils.Endianness endian) {
			if (maxBytes < 8) {
				stream.DiscardBytes(maxBytes);
				return null;
			}

			ulong identifier = stream.ReadUInt64(EndianUtils.Endianness.BigEndian);
			switch (identifier) {
				case 0:
					// archive has compression, but this file is not compressed
					stream.DiscardBytes(maxBytes - 8);
					return null;
				case DeflateSharpCompressionInfo.Identifier:
					return DeflateSharpCompressionInfo.Deserialize(stream, maxBytes - 8, endian);
				default:
					Console.WriteLine("Unknown compression type: " + identifier.ToString("x16"));
					stream.DiscardBytes(maxBytes - 8);
					return null;
			}
		}
	}
}
