using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaPluginBase.FileContainer {
	public interface IFile : INode {
		// stream of the contents of this file
		DuplicatableStream DataStream { get; }
	}
}
