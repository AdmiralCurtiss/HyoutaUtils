using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveBps {
		public static (byte[] patchInfo, byte[] patchData) CreatePatch(HyoutaArchiveBpsPatchInfo patchInfo, Stream data, EndianUtils.Endianness endian, byte packedAlignment) {
			throw new Exception("not yet implemented");
		}

		public static DuplicatableStream ApplyPatch(HyoutaArchiveBpsPatchInfo patchInfo, Stream data) {
			throw new Exception("not yet implemented");
		}
	}
}
