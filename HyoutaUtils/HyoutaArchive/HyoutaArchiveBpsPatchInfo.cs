using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveBpsPatchInfo {
		public HyoutaArchiveBpsPatchInfo(DuplicatableStream stream, long maxBytes, EndianUtils.Endianness endian, byte packedAlignment) {
			// TODO: implement a format for this
			stream.DiscardBytes(maxBytes);
		}

		public byte[] Serialize(EndianUtils.Endianness endian, byte packedAlignment) {
			throw new NotImplementedException();
		}
	}
}
