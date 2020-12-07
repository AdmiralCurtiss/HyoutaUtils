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
					(byte)((Part1 >> 56) & 0xff),
					(byte)((Part1 >> 48) & 0xff),
					(byte)((Part1 >> 40) & 0xff),
					(byte)((Part1 >> 32) & 0xff),
					(byte)((Part1 >> 24) & 0xff),
					(byte)((Part1 >> 16) & 0xff),
					(byte)((Part1 >> 8) & 0xff),
					(byte)(Part1 & 0xff),
					(byte)((Part2 >> 56) & 0xff),
					(byte)((Part2 >> 48) & 0xff),
					(byte)((Part2 >> 40) & 0xff),
					(byte)((Part2 >> 32) & 0xff),
					(byte)((Part2 >> 24) & 0xff),
					(byte)((Part2 >> 16) & 0xff),
					(byte)((Part2 >> 8) & 0xff),
					(byte)(Part2 & 0xff),
					(byte)((Part3 >> 24) & 0xff),
					(byte)((Part3 >> 16) & 0xff),
					(byte)((Part3 >> 8) & 0xff),
					(byte)(Part3 & 0xff),
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
			Part1 = (((ulong)(value[7])))
			      | (((ulong)(value[6])) << 8)
			      | (((ulong)(value[5])) << 16)
			      | (((ulong)(value[4])) << 24)
			      | (((ulong)(value[3])) << 32)
			      | (((ulong)(value[2])) << 40)
			      | (((ulong)(value[1])) << 48)
			      | (((ulong)(value[0])) << 56);
			Part2 = (((ulong)(value[15])))
			      | (((ulong)(value[14])) << 8)
			      | (((ulong)(value[13])) << 16)
			      | (((ulong)(value[12])) << 24)
			      | (((ulong)(value[11])) << 32)
			      | (((ulong)(value[10])) << 40)
			      | (((ulong)(value[9])) << 48)
			      | (((ulong)(value[8])) << 56);
			Part3 = (((uint)(value[19])))
			      | (((uint)(value[18])) << 8)
			      | (((uint)(value[17])) << 16)
			      | (((uint)(value[16])) << 24);
		}

		public SHA1(ulong part1, ulong part2, uint part3) {
			Part1 = part1;
			Part2 = part2;
			Part3 = part3;
		}

		public SHA1(string sha1) {
			if (sha1.Length != 40) {
				throw new Exception("incorrect length for SHA1 string");
			}

			Part1 = (((ulong)Decode4Bytes(sha1, 0)) << 32) | ((ulong)Decode4Bytes(sha1, 8));
			Part2 = (((ulong)Decode4Bytes(sha1, 16)) << 32) | ((ulong)Decode4Bytes(sha1, 24));
			Part3 = Decode4Bytes(sha1, 32);
		}

		public static uint Decode4Bytes(string s, int offset) {
			uint h = 0;
			for (int i = 0; i < 8; ++i) {
				char digit = s[offset + i];
				switch (digit) {
					case '0': h = (h << 4) | 0x0; break;
					case '1': h = (h << 4) | 0x1; break;
					case '2': h = (h << 4) | 0x2; break;
					case '3': h = (h << 4) | 0x3; break;
					case '4': h = (h << 4) | 0x4; break;
					case '5': h = (h << 4) | 0x5; break;
					case '6': h = (h << 4) | 0x6; break;
					case '7': h = (h << 4) | 0x7; break;
					case '8': h = (h << 4) | 0x8; break;
					case '9': h = (h << 4) | 0x9; break;
					case 'a': case 'A': h = (h << 4) | 0xa; break;
					case 'b': case 'B': h = (h << 4) | 0xb; break;
					case 'c': case 'C': h = (h << 4) | 0xc; break;
					case 'd': case 'D': h = (h << 4) | 0xd; break;
					case 'e': case 'E': h = (h << 4) | 0xe; break;
					case 'f': case 'F': h = (h << 4) | 0xf; break;
					default: throw new Exception("incorrect format for hex string");
				}
			}
			return h;
		}

		public bool Equals(SHA1 other) {
			return this == other;
		}

		public override bool Equals(object obj) {
			return obj is SHA1 && Equals((SHA1)obj);
		}

		public static bool operator ==(SHA1 lhs, SHA1 rhs) {
			return lhs.Part1 == rhs.Part1 && lhs.Part2 == rhs.Part2 && lhs.Part3 == rhs.Part3;
		}

		public static bool operator !=(SHA1 lhs, SHA1 rhs) {
			return !(lhs == rhs);
		}

		public override int GetHashCode() {
			return (int)Part3;
		}

		public override string ToString() {
			return string.Format("{0:x16}{1:x16}{2:x8}", Part1, Part2, Part3);
		}
	}
}
