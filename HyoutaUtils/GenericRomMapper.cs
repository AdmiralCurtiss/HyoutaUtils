using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils {
	public class GenericRomMapper : IRomMapper {
		public struct Region {
			public ulong RomStart;
			public ulong RamStart;
			public ulong Length;

			public Region(ulong romStart, ulong ramStart, ulong length) {
				RomStart = romStart;
				RamStart = ramStart;
				Length = length;
			}

			public override string ToString() {
				return string.Format("ROM: 0x{0:x16}, RAM: 0x{1:x16}, Length: 0x{2:x16}", RomStart, RamStart, Length);
			}
		}

		private Region[] Regions;

		public GenericRomMapper(IEnumerable<Region> regions) {
			Regions = regions.ToArray();
		}

		public bool TryMapRamToRom(ulong ramAddress, out ulong value) {
			foreach (Region r in Regions) {
				if (r.RamStart <= ramAddress && ramAddress < (r.RamStart + r.Length)) {
					value = ramAddress - (r.RamStart - r.RomStart);
					return true;
				}
			}
			value = 0;
			return false;
		}

		public bool TryMapRomToRam(ulong romAddress, out ulong value) {
			foreach (Region r in Regions) {
				if (r.RomStart <= romAddress && romAddress < (r.RomStart + r.Length)) {
					value = romAddress + (r.RamStart - r.RomStart);
					return true;
				}
			}
			value = 0;
			return false;
		}
	}
}
