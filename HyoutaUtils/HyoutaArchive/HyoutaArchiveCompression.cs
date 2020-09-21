using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveCompression {
		public static (byte[] compressionInfo, byte[] compressedData) Compress(HyoutaArchiveCompressionInfo compressionInfo, Stream data, byte packedAlignment) {
			throw new Exception("not yet implemented");
		}
	}
}
