using System;

namespace HyoutaPluginBase {
	public interface IRomMapper {
		bool TryMapRamToRom(uint ramAddress, out uint value);
		bool TryMapRomToRam(uint romAddress, out uint value);
	}
}
