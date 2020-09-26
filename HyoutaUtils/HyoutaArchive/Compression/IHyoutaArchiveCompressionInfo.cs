using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive.Compression {
	public interface IHyoutaArchiveCompressionInfo {
		byte[] Serialize(EndianUtils.Endianness endian);

		(byte[] compressionInfo, byte[] compressedData) Compress(Stream data, EndianUtils.Endianness endian);

		DuplicatableStream Decompress(Stream data);
	}
}
