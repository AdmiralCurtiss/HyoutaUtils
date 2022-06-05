using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaPluginBase.FileContainer {
	public interface INode : IDisposable {
		// true if this is a file
		// false if this is a container
		bool IsFile { get; }

		// true if this is a container
		// false if this is a false
		bool IsContainer { get; }

		// this node as a file if this is a file
		// null if this is a container
		IFile? AsFile { get; }

		// this node as a container if this is a container
		// null if this is a file
		IContainer? AsContainer { get; }
	}
}
