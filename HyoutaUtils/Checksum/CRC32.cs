using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.Checksum {
	public struct CRC32 : IEquatable<CRC32> {
		public uint Value { get; private set; }

		public CRC32(uint value) {
			Value = value;
		}

		public bool Equals(CRC32 other) {
			return Value == other.Value;
		}
	}
}
