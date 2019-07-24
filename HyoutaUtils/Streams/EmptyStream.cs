using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;

namespace HyoutaUtils.Streams {
	public class EmptyStream : DuplicatableStream {
		public static EmptyStream Instance = new EmptyStream();

		private EmptyStream() { }

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => 0;

		public override long Position {
			get => 0;
			set {
				if ( value < 0 ) {
					throw new Exception( "Invalid position for stream." );
				}
			}
		}

		public override void Flush() {
		}

		public override int ReadByte() {
			return -1;
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			return 0;
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			return 0;
		}

		public override void SetLength( long value ) {
			throw new Exception( "Cannot set length of empty stream." );
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			throw new Exception( "Cannot write to empty stream." );
		}

		public override DuplicatableStream Duplicate() {
			return this;
		}

		public override void ReStart() {
		}

		public override void End() {
		}
	}
}
