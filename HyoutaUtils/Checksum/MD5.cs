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
					(byte)((Half1 >> 56) & 0xff),
					(byte)((Half1 >> 48) & 0xff),
					(byte)((Half1 >> 40) & 0xff),
					(byte)((Half1 >> 32) & 0xff),
					(byte)((Half1 >> 24) & 0xff),
					(byte)((Half1 >> 16) & 0xff),
					(byte)((Half1 >> 8) & 0xff),
					(byte)(Half1 & 0xff),
					(byte)((Half2 >> 56) & 0xff),
					(byte)((Half2 >> 48) & 0xff),
					(byte)((Half2 >> 40) & 0xff),
					(byte)((Half2 >> 32) & 0xff),
					(byte)((Half2 >> 24) & 0xff),
					(byte)((Half2 >> 16) & 0xff),
					(byte)((Half2 >> 8) & 0xff),
					(byte)(Half2 & 0xff),
				};
			}
		}

		private ulong Half1;
		private ulong Half2;

		public MD5(byte[] value) {
			if (value.Length != 16) {
				throw new Exception("incorrect length for MD5 (must be 16 bytes)");
			}
			Half1 = (((ulong)(value[7])))
			      | (((ulong)(value[6])) << 8)
			      | (((ulong)(value[5])) << 16)
			      | (((ulong)(value[4])) << 24)
			      | (((ulong)(value[3])) << 32)
			      | (((ulong)(value[2])) << 40)
			      | (((ulong)(value[1])) << 48)
			      | (((ulong)(value[0])) << 56);
			Half2 = (((ulong)(value[15])))
			      | (((ulong)(value[14])) << 8)
			      | (((ulong)(value[13])) << 16)
			      | (((ulong)(value[12])) << 24)
			      | (((ulong)(value[11])) << 32)
			      | (((ulong)(value[10])) << 40)
			      | (((ulong)(value[9])) << 48)
			      | (((ulong)(value[8])) << 56);
		}

		public MD5(ulong half1, ulong half2) {
			Half1 = half1;
			Half2 = half2;
		}

		public MD5(string md5) {
			if (md5.Length != 32) {
				throw new Exception("incorrect length for MD5 string");
			}

			Half1 = (((ulong)SHA1.Decode4Bytes(md5, 0)) << 32) | ((ulong)SHA1.Decode4Bytes(md5, 8));
			Half2 = (((ulong)SHA1.Decode4Bytes(md5, 16)) << 32) | ((ulong)SHA1.Decode4Bytes(md5, 24));
		}

		public bool Equals(MD5 other) {
			return this == other;
		}

		public override bool Equals(object? obj) {
			return obj is MD5 && Equals((MD5)obj);
		}

		public static bool operator ==(MD5 lhs, MD5 rhs) {
			return lhs.Half1 == rhs.Half1 && lhs.Half2 == rhs.Half2;
		}

		public static bool operator !=(MD5 lhs, MD5 rhs) {
			return !(lhs == rhs);
		}

		public override int GetHashCode() {
			return (int)(uint)Half1;
		}

		public override string ToString() {
			return string.Format("{0:x16}{1:x16}", Half1, Half2);
		}
	}
}
