using System;

namespace HyoutaPluginBase {
	public interface IRomMapper {
		bool TryMapRamToRom(ulong ramAddress, out ulong value);
		bool TryMapRomToRam(ulong romAddress, out ulong value);
	}
}
