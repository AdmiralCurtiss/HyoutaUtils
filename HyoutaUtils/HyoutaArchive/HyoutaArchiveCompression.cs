using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveCompression {
		public static (byte[] compressionInfo, byte[] compressedData) Compress(HyoutaArchiveCompressionInfo compressionInfo, Stream data, EndianUtils.Endianness endian, byte packedAlignment) {
			throw new Exception("not yet implemented");
		}

		public static DuplicatableStream Decompress(HyoutaArchiveCompressionInfo compressionInfo, Stream data) {
			throw new Exception("not yet implemented");

			// compression info has the info on the compression type and the like
			// current position of 'data' has the compressed data
		}
	}
}
