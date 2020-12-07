using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.Checksum {
	public struct CRC32 : IEquatable<CRC32> {
		public uint Value { get; private set; }
		public byte[] Bytes {
			get {
				return new byte[4] {
					(byte)((Value >> 24) & 0xff),
					(byte)((Value >> 16) & 0xff),
					(byte)((Value >> 8) & 0xff),
					(byte)(Value & 0xff),
				};
			}
		}

		public CRC32(byte[] value) {
			if (value.Length != 4) {
				throw new Exception("incorrect length for CRC32 (must be 4 bytes)");
			}
			Value = (((uint)(value[3])))
			      | (((uint)(value[2])) << 8)
			      | (((uint)(value[1])) << 16)
			      | (((uint)(value[0])) << 24);
		}

		public CRC32(uint value) {
			Value = value;
		}

		public CRC32(string crc) {
			if (crc.Length != 8) {
				throw new Exception("incorrect length for CRC32 string");
			}

			Value = SHA1.Decode4Bytes(crc, 0);
		}

		public bool Equals(CRC32 other) {
			return this == other;
		}

		public override bool Equals(object obj) {
			return obj is CRC32 && Equals((CRC32)obj);
		}

		public static bool operator ==(CRC32 lhs, CRC32 rhs) {
			return lhs.Value == rhs.Value;
		}

		public static bool operator !=(CRC32 lhs, CRC32 rhs) {
			return !(lhs == rhs);
		}

		public override int GetHashCode() {
			return (int)Value;
		}

		public override string ToString() {
			return Value.ToString("x8");
		}
	}
}
