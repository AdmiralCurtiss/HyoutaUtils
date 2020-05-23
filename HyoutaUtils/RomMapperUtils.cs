using System;
using HyoutaPluginBase;

namespace HyoutaUtils {
	public static class RomMapperUtils {
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
