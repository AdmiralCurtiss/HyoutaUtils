using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveBpsPatchInfo {
		public ulong FileIndexToPatch;
		public ulong TargetFilesize;
		public HyoutaArchiveChunk ReferencedChunk;

		public HyoutaArchiveBpsPatchInfo(ulong index, ulong targetFilesize, HyoutaArchiveChunk referencedChunk) {
			FileIndexToPatch = index;
			TargetFilesize = targetFilesize;
			ReferencedChunk = referencedChunk;
		}

		public static HyoutaArchiveBpsPatchInfo Deserialize(DuplicatableStream stream, long maxBytes, EndianUtils.Endianness endian, ulong currentFileIndex, HyoutaArchiveChunk referencedChunk) {
			if (maxBytes < 16) {
				stream.DiscardBytes(maxBytes);
				return null;
			} else {
				ulong fileIndexToPatch = stream.ReadUInt64(endian);
				ulong targetFilesize = stream.ReadUInt64(endian);
				stream.DiscardBytes(maxBytes - 16);
				if (fileIndexToPatch == currentFileIndex) {
					// this is how you set a file to be unpatched in an archive that has patches
					return null;
				} else {
					return new HyoutaArchiveBpsPatchInfo(fileIndexToPatch, targetFilesize, referencedChunk);
				}
			}
		}

		public byte[] Serialize(EndianUtils.Endianness endian) {
			using (MemoryStream ms = new MemoryStream()) {
				ms.WriteUInt64(FileIndexToPatch, endian);
				ms.WriteUInt64(TargetFilesize, endian);
				return ms.CopyToByteArrayAndDispose();
			}
		}
	}
}
