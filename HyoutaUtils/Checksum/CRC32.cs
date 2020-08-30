using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.Checksum {
	public static class CRC32 {
		public static uint CalculateCRC32FromCurrentPosition(Stream s, long bytecount) {
			uint crc32 = CRC32Algorithm.crc_init();
			crc32 = CRC32Algorithm.crc_update(crc32, s, (ulong)bytecount);
			crc32 = CRC32Algorithm.crc_finalize(crc32);
			return crc32;
		}
	}
}
