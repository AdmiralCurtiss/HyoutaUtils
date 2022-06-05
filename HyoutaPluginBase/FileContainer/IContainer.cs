using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaPluginBase.FileContainer {
	public interface IContainer : INode {
		// returns a child node of this container from a given index
		// the exact meaning of index depends on the container, but typically
		// it's an integer from zero to child count minus one
		// returns null if the given index does not refer to a valid child
		INode? GetChildByIndex( long index );

		// returns a child node of this container from a given name
		// the exact meaning of name depends on the container, but typically
		// it's something like a filename
		// returns null if the given name does not refer to a valid child
		INode? GetChildByName( string name );

		// names of the children of this container, if any
		IEnumerable<string>? GetChildNames();
	}
}
