using HyoutaUtils.Checksum;
using System;
using System.IO;

namespace HyoutaUtils {
	public static class ChecksumUtils {
		public static CRC32 CalculateCRC32FromCurrentPosition(this Stream s, long bytecount) {
			if (bytecount < 0) {
				throw new Exception("negative bytecount is invalid");
			}

			return CalculateCRC32FromCurrentPosition(s, (ulong)bytecount);
		}

		public static CRC32 CalculateCRC32FromCurrentPosition(this Stream s, ulong bytecount) {
			uint crc32 = CRC32Algorithm.crc_init();
			crc32 = CRC32Algorithm.crc_update(crc32, s, bytecount);
			crc32 = CRC32Algorithm.crc_finalize(crc32);
			return new CRC32(crc32);
		}

		public static CRC32 CalculateCRC32ForEntireStream(this Stream s) {
			long p = s.Position;
			s.Position = 0;
			CRC32 crc32 = CalculateCRC32FromCurrentPosition(s, s.Length);
			s.Position = p;
			return crc32;
		}

		public static MD5 CalculateMD5ForEntireStream(this Stream s) {
			using (var algorithm = System.Security.Cryptography.MD5.Create()) {
				long p = s.Position;
				s.Position = 0;
				byte[] hash = algorithm.ComputeHash(s);
				s.Position = p;
				return new MD5(hash);
			}
		}

		public static SHA1 CalculateSHA1ForEntireStream(this Stream s) {
			using (var algorithm = System.Security.Cryptography.SHA1.Create()) {
				long p = s.Position;
				s.Position = 0;
				byte[] hash = algorithm.ComputeHash(s);
				s.Position = p;
				return new SHA1(hash);
			}
		}
	}
}
