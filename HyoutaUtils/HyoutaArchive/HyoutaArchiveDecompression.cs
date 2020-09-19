using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveDecompression {
		public static DuplicatableStream Decompress(HyoutaArchiveCompressionInfo compressionInfo, DuplicatableStream data) {
			throw new Exception("not yet implemented");

			// compression info has the info on the compression type and the like
			// current position of 'data' has the compressed data
		}
	}
}
