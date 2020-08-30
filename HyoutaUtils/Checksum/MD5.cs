using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.Checksum {
	public struct MD5 : IEquatable<MD5> {
		public byte[] Value {
			get {
				return new byte[16] {
					(byte)(Half1 & 0xff),
					(byte)((Half1 >> 8) & 0xff),
					(byte)((Half1 >> 16) & 0xff),
					(byte)((Half1 >> 24) & 0xff),
					(byte)((Half1 >> 32) & 0xff),
					(byte)((Half1 >> 40) & 0xff),
					(byte)((Half1 >> 48) & 0xff),
					(byte)((Half1 >> 56) & 0xff),
					(byte)(Half2 & 0xff),
					(byte)((Half2 >> 8) & 0xff),
					(byte)((Half2 >> 16) & 0xff),
					(byte)((Half2 >> 24) & 0xff),
					(byte)((Half2 >> 32) & 0xff),
					(byte)((Half2 >> 40) & 0xff),
					(byte)((Half2 >> 48) & 0xff),
					(byte)((Half2 >> 56) & 0xff),
				};
			}
		}

		private ulong Half1;
		private ulong Half2;

		public MD5(byte[] value) {
			if (value.Length != 16) {
				throw new Exception("incorrect length for MD5 (must be 16 bytes)");
			}
			Half1 = (((ulong)(value[0])))
			      | (((ulong)(value[1])) << 8)
			      | (((ulong)(value[2])) << 16)
			      | (((ulong)(value[3])) << 24)
			      | (((ulong)(value[4])) << 32)
			      | (((ulong)(value[5])) << 40)
			      | (((ulong)(value[6])) << 48)
			      | (((ulong)(value[7])) << 56);
			Half2 = (((ulong)(value[8])))
			      | (((ulong)(value[9])) << 8)
			      | (((ulong)(value[10])) << 16)
			      | (((ulong)(value[11])) << 24)
			      | (((ulong)(value[12])) << 32)
			      | (((ulong)(value[13])) << 40)
			      | (((ulong)(value[14])) << 48)
			      | (((ulong)(value[15])) << 56);
		}

		public bool Equals(MD5 other) {
			return Half1 == other.Half1 && Half2 == other.Half2;
		}
	}
}
