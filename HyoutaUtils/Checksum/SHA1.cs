using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.Checksum {
	public struct SHA1 : IEquatable<SHA1> {
		public byte[] Value {
			get {
				return new byte[] {
					(byte)(Part1 & 0xff),
					(byte)((Part1 >> 8) & 0xff),
					(byte)((Part1 >> 16) & 0xff),
					(byte)((Part1 >> 24) & 0xff),
					(byte)((Part1 >> 32) & 0xff),
					(byte)((Part1 >> 40) & 0xff),
					(byte)((Part1 >> 48) & 0xff),
					(byte)((Part1 >> 56) & 0xff),
					(byte)(Part2 & 0xff),
					(byte)((Part2 >> 8) & 0xff),
					(byte)((Part2 >> 16) & 0xff),
					(byte)((Part2 >> 24) & 0xff),
					(byte)((Part2 >> 32) & 0xff),
					(byte)((Part2 >> 40) & 0xff),
					(byte)((Part2 >> 48) & 0xff),
					(byte)((Part2 >> 56) & 0xff),
					(byte)(Part3 & 0xff),
					(byte)((Part3 >> 8) & 0xff),
					(byte)((Part3 >> 16) & 0xff),
					(byte)((Part3 >> 24) & 0xff),
				};
			}
		}

		private ulong Part1;
		private ulong Part2;
		private uint Part3;

		public SHA1(byte[] value) {
			if (value.Length != 20) {
				throw new Exception("incorrect length for SHA1 (must be 20 bytes)");
			}
			Part1 = (((ulong)(value[0])))
			      | (((ulong)(value[1])) << 8)
			      | (((ulong)(value[2])) << 16)
			      | (((ulong)(value[3])) << 24)
			      | (((ulong)(value[4])) << 32)
			      | (((ulong)(value[5])) << 40)
			      | (((ulong)(value[6])) << 48)
			      | (((ulong)(value[7])) << 56);
			Part2 = (((ulong)(value[8])))
			      | (((ulong)(value[9])) << 8)
			      | (((ulong)(value[10])) << 16)
			      | (((ulong)(value[11])) << 24)
			      | (((ulong)(value[12])) << 32)
			      | (((ulong)(value[13])) << 40)
			      | (((ulong)(value[14])) << 48)
			      | (((ulong)(value[15])) << 56);
			Part3 = (((uint)(value[16])))
			      | (((uint)(value[17])) << 8)
			      | (((uint)(value[18])) << 16)
			      | (((uint)(value[19])) << 24);
		}

		public bool Equals(SHA1 other) {
			return Part1 == other.Part1 && Part2 == other.Part2 && Part3 == other.Part3;
		}
	}
}
