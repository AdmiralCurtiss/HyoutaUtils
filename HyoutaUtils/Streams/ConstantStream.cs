using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;

namespace HyoutaUtils.Streams {
	public class ConstantStream : DuplicatableStream {
		private long InternalPosition;
		private long InternalSize;
		private byte InternalConstant;

		public ConstantStream( long size, byte constant ) {
			InternalPosition = 0;
			InternalSize = size;
			InternalConstant = constant;
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => InternalSize;

		public override long Position {
			get => InternalPosition;
			set {
				if ( value < 0 ) {
					throw new Exception( "Invalid position for partial stream." );
				}
				InternalPosition = Math.Min( value, InternalSize );
			}
		}

		public override void Flush() {
		}

		public override int ReadByte() {
			if ( InternalPosition < InternalSize ) {
				++InternalPosition;
				return InternalConstant;
			}
			return -1;
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			int c = (int)Math.Min( InternalSize - InternalPosition, count );
			for ( int i = 0; i < c; ++i ) {
				buffer[offset + i] = InternalConstant;
			}
			InternalPosition += c;
			return c;
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			switch ( origin ) {
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					Position = Position + offset;
					break;
				case SeekOrigin.End:
					Position = Length - offset; // FIXME: is that right?
					break;
			}
			return Position;
		}

		public override void SetLength( long value ) {
			throw new Exception( "Cannot set length of constant streams." );
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			throw new Exception( "Cannot write to constant streams." );
		}

		public override DuplicatableStream Duplicate() {
			return new ConstantStream( InternalSize, InternalConstant );
		}

		public override void ReStart() {
			InternalPosition = 0;
		}

		public override void End() {
			InternalPosition = 0;
		}
	}
}
