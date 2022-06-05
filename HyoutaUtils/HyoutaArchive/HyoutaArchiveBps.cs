using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public static class HyoutaArchiveBps {
		public static (byte[] patchInfo, byte[] patchData) CreatePatch(HyoutaArchiveBpsPatchInfo patchInfo, Stream data, EndianUtils.Endianness endian) {
			throw new Exception("not yet implemented");
		}

		public static DuplicatableStream ApplyPatch(HyoutaArchiveBpsPatchInfo patchInfo, Stream data) {
			var refchunk = patchInfo.ReferencedChunk;
			if (refchunk == null) {
				// implies that the file points at itself and is unpatched
				return data.CopyToByteArrayStream();
			}

			HyoutaArchiveFileInfo sourceFile = refchunk.GetFile((long)patchInfo.FileIndexToPatch);
			using (DuplicatableStream sourceStream = sourceFile.DataStream.Duplicate()) {
				sourceStream.Position = 0;
				data.Position = 0;
				using (MemoryStream ms = new MemoryStream((int)patchInfo.TargetFilesize)) {
					Bps.BpsPatcher.ApplyPatchToStream(sourceStream, data, ms);
					return ms.CopyToByteArrayStreamAndDispose();
				}
			}
		}
	}
}
