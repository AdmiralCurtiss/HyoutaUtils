using System;
using HyoutaPluginBase;

namespace HyoutaUtils {
	public static class RomMapperUtils {
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
	}
}
