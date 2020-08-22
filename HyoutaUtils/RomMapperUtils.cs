using System;
using HyoutaPluginBase;

namespace HyoutaUtils {
	public static class RomMapperUtils {
		public static bool TryMapRamToRom(this IRomMapper mapper, uint ramAddress, out uint value) {
			ulong i = ramAddress;
			ulong o;
			bool b = mapper.TryMapRamToRom(i, out o);
			value = (uint)(o & 0xffffffffu);
			return b;
		}

		public static bool TryMapRomToRam(this IRomMapper mapper, uint romAddress, out uint value) {
			ulong i = romAddress;
			ulong o;
			bool b = mapper.TryMapRomToRam(i, out o);
			value = (uint)(o & 0xffffffffu);
			return b;
		}

		public static ulong MapRamToRom(this IRomMapper mapper, ulong ramAddress) {
			ulong v;
			if (mapper.TryMapRamToRom(ramAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}

		public static ulong MapRomToRam(this IRomMapper mapper, ulong romAddress) {
			ulong v;
			if (mapper.TryMapRomToRam(romAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}

		public static uint MapRamToRom(this IRomMapper mapper, uint ramAddress) {
			uint v;
			if (mapper.TryMapRamToRom(ramAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}

		public static uint MapRomToRam(this IRomMapper mapper, uint romAddress) {
			uint v;
			if (mapper.TryMapRomToRam(romAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}
	}
}
