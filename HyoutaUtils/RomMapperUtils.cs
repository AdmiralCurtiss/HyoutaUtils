using System;
using HyoutaPluginBase;

namespace HyoutaUtils {
	public static class RomMapperUtils {
		public static bool TryMapRamToRom(this IRomMapper mapper, long ramAddress, out long value) {
			ulong i = (ulong)ramAddress;
			ulong o;
			bool b = mapper.TryMapRamToRom(i, out o);
			value = (long)o;
			return b;
		}

		public static bool TryMapRomToRam(this IRomMapper mapper, long romAddress, out long value) {
			ulong i = (ulong)romAddress;
			ulong o;
			bool b = mapper.TryMapRomToRam(i, out o);
			value = (long)o;
			return b;
		}

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

		public static bool TryMapRamToRom(this IRomMapper mapper, int ramAddress, out int value) {
			ulong i = (uint)ramAddress;
			ulong o;
			bool b = mapper.TryMapRamToRom(i, out o);
			value = (int)(uint)(o & 0xffffffffu);
			return b;
		}

		public static bool TryMapRomToRam(this IRomMapper mapper, int romAddress, out int value) {
			ulong i = (uint)romAddress;
			ulong o;
			bool b = mapper.TryMapRomToRam(i, out o);
			value = (int)(uint)(o & 0xffffffffu);
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

		public static long MapRamToRom(this IRomMapper mapper, long ramAddress) {
			long v;
			if (mapper.TryMapRamToRom(ramAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}

		public static long MapRomToRam(this IRomMapper mapper, long romAddress) {
			long v;
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

		public static int MapRamToRom(this IRomMapper mapper, int ramAddress) {
			int v;
			if (mapper.TryMapRamToRom(ramAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}

		public static int MapRomToRam(this IRomMapper mapper, int romAddress) {
			int v;
			if (mapper.TryMapRomToRam(romAddress, out v)) {
				return v;
			}
			throw new Exception("Address not mappable.");
		}
	}
}
