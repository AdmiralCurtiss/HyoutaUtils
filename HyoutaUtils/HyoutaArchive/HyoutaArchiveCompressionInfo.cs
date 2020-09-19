using HyoutaPluginBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveCompressionInfo {
		public HyoutaArchiveCompressionInfo(DuplicatableStream stream, long maxBytes) {
			// TODO: implement a format for this
			stream.DiscardBytes(maxBytes);
		}
	}
}
