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
			HyoutaArchiveFileInfo sourceFile = patchInfo.ReferencedChunk.GetFile((long)patchInfo.FileIndexToPatch);
			using (DuplicatableStream sourceStream = sourceFile.DataStream.Duplicate()) {
				sourceStream.Position = 0;
				data.Position = 0;
				using (MemoryStream ms = new MemoryStream()) {
					Bps.BpsPatcher.ApplyPatchToStream(sourceStream, data, ms);
					return ms.CopyToByteArrayStreamAndDispose();
				}
			}
		}
	}
}
