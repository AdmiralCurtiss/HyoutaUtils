using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils {
	public static class StreamUtils {
		public static void CopyStream(Stream input, Stream output) {
			CopyStream(input, output, input.Length - input.Position);
		}

		public static void CopyStream(Stream input, Stream output, long count) {
			CopyStream(input, output, (ulong)count);
		}

		public static void CopyStream(Stream input, Stream output, ulong count) {
			const ulong bufferSize = 4096;
			byte[] buffer = new byte[bufferSize];
			int read;

			ulong bytesLeft = count;
			while ((read = input.Read(buffer, 0, (int)Math.Min(bufferSize, bytesLeft))) > 0) {
				output.Write(buffer, 0, read);
				bytesLeft -= (ulong)read;
				if (bytesLeft <= 0) return;
			}
		}

		public static bool IsIdentical( this Stream str, Stream other, long count ) {
			for ( long i = 0; i < count; ++i ) {
				if ( str.ReadByte() != other.ReadByte() ) {
					return false;
				}
			}
			return true;
		}

		public static ulong ReadUInt64(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();
			ulong b7 = (ulong)s.ReadByte();
			ulong b8 = (ulong)s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (ulong)(b8 << 56 | b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (ulong)(b1 << 56 | b2 << 48 | b3 << 40 | b4 << 32 | b5 << 24 | b6 << 16 | b7 << 8 | b8);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static ulong ReadUInt56(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();
			ulong b7 = (ulong)s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (ulong)(b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (ulong)(b1 << 48 | b2 << 40 | b3 << 32 | b4 << 24 | b5 << 16 | b6 << 8 | b7);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static ulong ReadUInt48(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (ulong)(b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (ulong)(b1 << 40 | b2 << 32 | b3 << 24 | b4 << 16 | b5 << 8 | b6);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static ulong ReadUInt40(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (ulong)(b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (ulong)(b1 << 32 | b2 << 24 | b3 << 16 | b4 << 8 | b5);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static uint ReadUInt32(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();
			int b4 = s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (uint)(b4 << 24 | b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (uint)(b1 << 24 | b2 << 16 | b3 << 8 | b4);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static uint ReadUInt24(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (uint)(b3 << 16 | b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (uint)(b1 << 16 | b2 << 8 | b3);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static ushort ReadUInt16(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();

			switch (endian) {
				case EndianUtils.Endianness.LittleEndian:
					return (ushort)(b2 << 8 | b1);
				case EndianUtils.Endianness.BigEndian:
					return (ushort)(b1 << 8 | b2);
				default:
					throw new Exception("unknown endianness");
			}
		}

		public static byte ReadUInt8(this Stream s) {
			return Convert.ToByte(s.ReadByte());
		}

		public static long ReadInt64(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (long)ReadUInt64(s, endian);
		}

		public static int ReadInt32(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (int)ReadUInt32(s, endian);
		}

		public static short ReadInt16(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (short)ReadUInt16(s, endian);
		}

		public static sbyte ReadInt8(this Stream s) {
			return (sbyte)ReadUInt8(s);
		}

		public static ulong PeekUInt64(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			ulong retval = s.ReadUInt64(endian);
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt56(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			ulong retval = s.ReadUInt56(endian);
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt48(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			ulong retval = s.ReadUInt48(endian);
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt40(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			ulong retval = s.ReadUInt40(endian);
			s.Position = pos;
			return retval;
		}

		public static uint PeekUInt32(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			uint retval = s.ReadUInt32(endian);
			s.Position = pos;
			return retval;
		}

		public static uint PeekUInt24(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			uint retval = s.ReadUInt24(endian);
			s.Position = pos;
			return retval;
		}

		public static ushort PeekUInt16(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			long pos = s.Position;
			ushort retval = s.ReadUInt16(endian);
			s.Position = pos;
			return retval;
		}

		public static byte PeekUInt8(this Stream s) {
			long pos = s.Position;
			byte retval = s.ReadUInt8();
			s.Position = pos;
			return retval;
		}

		public static long PeekInt64(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (long)PeekUInt64(s, endian);
		}

		public static int PeekInt32(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (int)PeekUInt32(s, endian);
		}

		public static short PeekInt16(this Stream s, EndianUtils.Endianness endian = EndianUtils.Endianness.LittleEndian) {
			return (short)PeekUInt16(s, endian);
		}

		public static sbyte PeekInt8(this Stream s) {
			return (sbyte)PeekUInt8(s);
		}

		public static void WriteUInt64( this Stream s, ulong num ) {
			s.Write( BitConverter.GetBytes( num ), 0, 8 );
		}

		public static void WriteUInt32( this Stream s, uint num ) {
			s.Write( BitConverter.GetBytes( num ), 0, 4 );
		}

		public static void WriteUInt16( this Stream s, ushort num ) {
			s.Write( BitConverter.GetBytes( num ), 0, 2 );
		}

		public static void DiscardBytes( this Stream s, uint count ) {
			s.Position = s.Position + count;
		}

		public static byte[] ReadBytes( this Stream stream, long count ) {
			byte[] sd = new byte[count];
			stream.Read( sd, 0, sd.Length );
			return sd;
		}

		public static byte[] ReadUInt8Array( this Stream s, long count ) {
			// TODO: Isn't this just the same as ReadBytes() except slower?
			byte[] data = new byte[count];
			for ( long i = 0; i < count; ++i ) {
				data[i] = s.ReadUInt8();
			}
			return data;
		}

		public static uint[] ReadUInt32Array( this Stream s, long count, EndianUtils.Endianness endianness = EndianUtils.Endianness.LittleEndian ) {
			uint[] data = new uint[count];
			for ( long i = 0; i < count; ++i ) {
				data[i] = s.ReadUInt32(endianness);
			}
			return data;
		}

		public static ulong ReadUInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.ReadUInt8();
				case BitUtils.Bitness.B16: return s.ReadUInt16(endian);
				case BitUtils.Bitness.B32: return s.ReadUInt32(endian);
				case BitUtils.Bitness.B64: return s.ReadUInt64(endian);
			}
			throw new Exception( "Reading uint not implemented for bitness " + bits.ToString() );
		}

		public static long ReadInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.ReadInt8();
				case BitUtils.Bitness.B16: return s.ReadInt16(endian);
				case BitUtils.Bitness.B32: return s.ReadInt32(endian);
				case BitUtils.Bitness.B64: return s.ReadInt64(endian);
			}
			throw new Exception( "Reading int not implemented for bitness " + bits.ToString() );
		}

		public static ulong PeekUInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.PeekUInt8();
				case BitUtils.Bitness.B16: return s.PeekUInt16(endian);
				case BitUtils.Bitness.B32: return s.PeekUInt32(endian);
				case BitUtils.Bitness.B64: return s.PeekUInt64(endian);
			}
			throw new Exception( "Peeking uint not implemented for bitness " + bits.ToString() );
		}

		public static long PeekInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.PeekInt8();
				case BitUtils.Bitness.B16: return s.PeekInt16(endian);
				case BitUtils.Bitness.B32: return s.PeekInt32(endian);
				case BitUtils.Bitness.B64: return s.PeekInt64(endian);
			}
			throw new Exception( "Peeking int not implemented for bitness " + bits.ToString() );
		}

		public static void ReadAlign( this Stream s, long alignment ) {
			while ( s.Position % alignment != 0 ) {
				s.DiscardBytes( 1 );
			}
		}

		public static void WriteAlign( this Stream s, long alignment, byte paddingByte = 0 ) {
			while ( s.Position % alignment != 0 ) {
				s.WriteByte( paddingByte );
			}
		}

		public static string ReadNulltermStringFromLocationAndReset( this Stream s, ulong location, TextUtils.GameTextEncoding encoding ) {
			return ReadNulltermStringFromLocationAndReset( s, (long)location, encoding );
		}

		public static string ReadNulltermStringFromLocationAndReset( this Stream s, long location, TextUtils.GameTextEncoding encoding ) {
			long pos = s.Position;
			s.Position = location;
			string str = s.ReadNulltermString( encoding );
			s.Position = pos;
			return str;
		}

		public static string ReadNulltermString( this Stream s, TextUtils.GameTextEncoding encoding ) {
			switch ( encoding ) {
				case TextUtils.GameTextEncoding.ASCII: return ReadAsciiNullterm( s );
				case TextUtils.GameTextEncoding.ShiftJIS: return ReadShiftJisNullterm( s );
				case TextUtils.GameTextEncoding.UTF8: return ReadUTF8Nullterm( s );
				case TextUtils.GameTextEncoding.UTF16: return ReadUTF16Nullterm( s );
			}
			throw new Exception( "Reading nullterminated string not implemented for encoding " + encoding.ToString() );
		}

		public static string ReadSizedString( this Stream s, long count, TextUtils.GameTextEncoding encoding ) {
			switch ( encoding ) {
				case TextUtils.GameTextEncoding.ASCII: return ReadSizedString( s, count, Encoding.ASCII );
				case TextUtils.GameTextEncoding.ShiftJIS: return ReadSizedString( s, count, TextUtils.ShiftJISEncoding );
				case TextUtils.GameTextEncoding.UTF8: return ReadSizedString( s, count, Encoding.UTF8 );
				case TextUtils.GameTextEncoding.UTF16: return ReadSizedString( s, count, Encoding.Unicode );
			}
			throw new Exception( "Reading sized string not implemented for encoding " + encoding.ToString() );
		}

		public static string ReadSizedString( this Stream s, long count, Encoding encoding ) {
			byte[] data = new byte[count];
			s.Read( data, 0, (int)count ); // FIXME: actual long size here, if we somehow ever find a >2GB string
			return encoding.GetString( data );
		}

		public static void WriteNulltermString( this Stream s, string str, TextUtils.GameTextEncoding encoding ) {
			switch ( encoding ) {
				case TextUtils.GameTextEncoding.ASCII:
					WriteAsciiNullterm( s, str ); return;
				case TextUtils.GameTextEncoding.ShiftJIS:
					WriteShiftJisNullterm( s, str ); return;
				case TextUtils.GameTextEncoding.UTF8:
					WriteUTF8Nullterm( s, str ); return;
				case TextUtils.GameTextEncoding.UTF16:
					WriteUTF16Nullterm( s, str ); return;
			}
			throw new Exception( "Writing nullterminated string not implemented for encoding " + encoding.ToString() );
		}

		public static string ReadAsciiNulltermFromLocationAndReset( this Stream s, long location ) {
			long pos = s.Position;
			s.Position = location;
			string str = s.ReadAsciiNullterm();
			s.Position = pos;
			return str;
		}

		public static string ReadAsciiNullterm( this Stream s ) {
			StringBuilder sb = new StringBuilder();
			int b = s.ReadByte();
			while ( b != 0 && b != -1 ) {
				sb.Append( (char)( b ) );
				b = s.ReadByte();
			}
			return sb.ToString();
		}

		public static string ReadAscii( this Stream s, long count ) {
			return ReadSizedString( s, count, Encoding.ASCII );
		}

		public static void WriteAscii( this Stream s, string str, int count = 0, bool trim = false ) {
			WriteString( s, Encoding.ASCII, str, count, trim );
		}

		public static void WriteString( this Stream s, Encoding encoding, string str, int count = 0, bool trim = false ) {
			byte[] chars = encoding.GetBytes( str );
			if ( !trim && count > 0 && count < chars.Length ) {
				throw new Exception( "String won't fit in provided space!" );
			}

			int i;
			for ( i = 0; i < chars.Length; ++i ) {
				s.WriteByte( chars[i] );
			}
			for ( ; i < count; ++i ) {
				s.WriteByte( 0 );
			}
		}

		public static string ReadUTF8NulltermFromLocationAndReset( this Stream s, long location ) {
			long pos = s.Position;
			s.Position = location;
			string str = s.ReadUTF8Nullterm();
			s.Position = pos;
			return str;
		}
		public static string ReadUTF8Nullterm( this Stream s ) {
			List<byte> data = new List<byte>();
			int b = s.ReadByte();
			while ( b != 0 && b != -1 ) {
				data.Add( (byte)( b ) );
				b = s.ReadByte();
			}
			return Encoding.UTF8.GetString( data.ToArray() );
		}

		public static void WriteUTF8( this Stream s, string str, int count = 0, bool trim = false ) {
			WriteString( s, Encoding.UTF8, str, count, trim );
		}

		public static void WriteUTF8Nullterm( this Stream s, string str ) {
			WriteUTF8( s, str, 0, false );
			s.WriteByte( 0 );
		}

		public static void WriteUTF16( this Stream s, string str, int count = 0, bool trim = false ) {
			WriteString( s, Encoding.Unicode, str, count, trim );
		}

		public static void WriteUTF16Nullterm( this Stream s, string str ) {
			WriteUTF16( s, str, 0, false );
			s.WriteByte( 0 );
		}

		public static void WriteAsciiNullterm( this Stream s, string str ) {
			WriteAscii( s, str, 0, false );
			s.WriteByte( 0 );
		}

		public static void WriteShiftJis( this Stream s, string str, int count = 0, bool trim = false ) {
			WriteString( s, TextUtils.ShiftJISEncoding, str, count, trim );
		}

		public static void WriteShiftJisNullterm( this Stream s, string str ) {
			WriteShiftJis( s, str, 0, false );
			s.WriteByte( 0 );
		}

		public static string ReadUTF16Nullterm( this Stream s ) {
			StringBuilder sb = new StringBuilder();
			byte[] b = new byte[2];
			int b0 = s.ReadByte();
			int b1 = s.ReadByte();
			while ( !( b0 == 0 && b1 == 0 ) && b1 != -1 ) {
				b[0] = (byte)b0; b[1] = (byte)b1;
				sb.Append( Encoding.Unicode.GetString( b, 0, 2 ) );
				b0 = s.ReadByte(); b1 = s.ReadByte();
			}
			return sb.ToString();
		}

		public static string ReadShiftJis( this Stream s, long bytecount ) {
			return s.ReadSizedString( bytecount, TextUtils.GameTextEncoding.ShiftJIS );
		}

		public static string ReadShiftJisNullterm( this Stream s ) {
			StringBuilder sb = new StringBuilder();
			byte[] buffer = new byte[2];

			int b = s.ReadByte();
			while ( b != 0 && b != -1 ) {
				if ( ( b >= 0 && b <= 0x80 ) || ( b >= 0xA0 && b <= 0xDF ) ) {
					// is a single byte
					buffer[0] = (byte)b;
					sb.Append(TextUtils.ShiftJISEncoding.GetString( buffer, 0, 1 ) );
				} else {
					// is two bytes
					buffer[0] = (byte)b;
					buffer[1] = (byte)s.ReadByte();
					sb.Append(TextUtils.ShiftJISEncoding.GetString( buffer ) );
				}
				b = s.ReadByte();
			}
			return sb.ToString();
		}

		public static void Write( this Stream s, byte[] data ) {
			s.Write( data, 0, data.Length );
		}

		public static MemoryStream CopyToMemory( this Stream s ) {
			long p = s.Position;
			s.Position = 0;
			MemoryStream ms = new MemoryStream( (int)s.Length );
			CopyStream( s, ms, s.Length );
			s.Position = p;
			ms.Position = p;
			return ms;
		}

		public static MemoryStream CopyToMemoryAndDispose( this Stream s ) {
			long p = s.Position;
			if (p != 0) {
				s.Position = 0;
			}
			MemoryStream ms = new MemoryStream( (int)s.Length );
			CopyStream( s, ms, s.Length );
			ms.Position = p;
			s.Dispose();
			return ms;
		}

		public static byte[] CopyToByteArray( this Stream s ) {
			long p = s.Position;
			s.Position = 0;
			byte[] data = new byte[s.Length];
			s.Read(data, 0, (int)s.Length);
			s.Position = p;
			return data;
		}

		public static byte[] CopyToByteArrayAndDispose( this Stream s ) {
			if (s.Position != 0) {
				s.Position = 0;
			}
			byte[] data = new byte[s.Length];
			s.Read(data, 0, (int)s.Length);
			s.Dispose();
			return data;
		}
	}
}
