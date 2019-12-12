using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils {
	public static class StreamUtils {
		public static void CopyStream( System.IO.Stream input, System.IO.Stream output, long count ) {
			byte[] buffer = new byte[4096];
			int read;

			long bytesLeft = count;
			while ( ( read = input.Read( buffer, 0, (int)Math.Min( buffer.LongLength, bytesLeft ) ) ) > 0 ) {
				output.Write( buffer, 0, read );
				bytesLeft -= read;
				if ( bytesLeft <= 0 ) return;
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

		public static ulong ReadUInt64( this Stream s ) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();
			ulong b7 = (ulong)s.ReadByte();
			ulong b8 = (ulong)s.ReadByte();

			return (ulong)( b8 << 56 | b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
		}

		public static ulong ReadUInt56( this Stream s ) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();
			ulong b7 = (ulong)s.ReadByte();

			return (ulong)( b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
		}

		public static ulong ReadUInt48( this Stream s ) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();
			ulong b6 = (ulong)s.ReadByte();

			return (ulong)( b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
		}

		public static ulong ReadUInt40( this Stream s ) {
			ulong b1 = (ulong)s.ReadByte();
			ulong b2 = (ulong)s.ReadByte();
			ulong b3 = (ulong)s.ReadByte();
			ulong b4 = (ulong)s.ReadByte();
			ulong b5 = (ulong)s.ReadByte();

			return (ulong)( b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
		}

		public static uint ReadUInt32( this Stream s ) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();
			int b4 = s.ReadByte();

			return (uint)( b4 << 24 | b3 << 16 | b2 << 8 | b1 );
		}

		public static uint ReadUInt24( this Stream s ) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();

			return (uint)( b3 << 16 | b2 << 8 | b1 );
		}

		public static ushort ReadUInt16( this Stream s ) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();

			return (ushort)( b2 << 8 | b1 );
		}

		public static byte ReadUInt8( this Stream s ) {
			return Convert.ToByte( s.ReadByte() );
		}

		public static long ReadInt64( this Stream s ) {
			return (long) ReadUInt64( s );
		}

		public static long ReadInt56( this Stream s ) {
			return (long) ReadUInt56( s );
		}

		public static long ReadInt48( this Stream s ) {
			return (long) ReadUInt48( s );
		}

		public static long ReadInt40( this Stream s ) {
			return (long) ReadUInt40( s );
		}

		public static int ReadInt32( this Stream s ) {
			return (int) ReadUInt32( s );
		}

		public static int ReadInt24( this Stream s ) {
			return (int) ReadUInt24( s );
		}

		public static short ReadInt16( this Stream s ) {
			return (short) ReadUInt16( s );
		}

		public static sbyte ReadInt8( this Stream s ) {
			return (sbyte) ReadUInt8( s );
		}

		public static ulong PeekUInt64( this Stream s ) {
			long pos = s.Position;
			ulong retval = s.ReadUInt64();
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt56( this Stream s ) {
			long pos = s.Position;
			ulong retval = s.ReadUInt56();
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt48( this Stream s ) {
			long pos = s.Position;
			ulong retval = s.ReadUInt48();
			s.Position = pos;
			return retval;
		}

		public static ulong PeekUInt40( this Stream s ) {
			long pos = s.Position;
			ulong retval = s.ReadUInt40();
			s.Position = pos;
			return retval;
		}

		public static uint PeekUInt32( this Stream s ) {
			long pos = s.Position;
			uint retval = s.ReadUInt32();
			s.Position = pos;
			return retval;
		}

		public static uint PeekUInt24( this Stream s ) {
			long pos = s.Position;
			uint retval = s.ReadUInt24();
			s.Position = pos;
			return retval;
		}

		public static ushort PeekUInt16( this Stream s ) {
			long pos = s.Position;
			ushort retval = s.ReadUInt16();
			s.Position = pos;
			return retval;
		}

		public static byte PeekUInt8( this Stream s ) {
			long pos = s.Position;
			byte retval = s.ReadUInt8();
			s.Position = pos;
			return retval;
		}

		public static long PeekInt64( this Stream s ) {
			return (long) PeekUInt64( s );
		}

		public static long PeekInt56( this Stream s ) {
			return (long) PeekUInt56( s );
		}

		public static long PeekInt48( this Stream s ) {
			return (long) PeekUInt48( s );
		}

		public static long PeekInt40( this Stream s ) {
			return (long) PeekUInt40( s );
		}

		public static int PeekInt32( this Stream s ) {
			return (int) PeekUInt32( s );
		}

		public static int PeekInt24( this Stream s ) {
			return (int) PeekUInt24( s );
		}

		public static short PeekInt16( this Stream s ) {
			return (short) PeekUInt16( s );
		}

		public static sbyte PeekInt8( this Stream s ) {
			return (sbyte) PeekUInt8( s );
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
				data[i] = s.ReadUInt32().FromEndian( endianness );
			}
			return data;
		}

		public static ulong ReadUInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.ReadUInt8();
				case BitUtils.Bitness.B16: return s.ReadUInt16().FromEndian( endian );
				case BitUtils.Bitness.B32: return s.ReadUInt32().FromEndian( endian );
				case BitUtils.Bitness.B64: return s.ReadUInt64().FromEndian( endian );
			}
			throw new Exception( "Reading uint not implemented for bitness " + bits.ToString() );
		}

		public static long ReadInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.ReadInt8();
				case BitUtils.Bitness.B16: return s.ReadInt16().FromEndian( endian );
				case BitUtils.Bitness.B32: return s.ReadInt32().FromEndian( endian );
				case BitUtils.Bitness.B64: return s.ReadInt64().FromEndian( endian );
			}
			throw new Exception( "Reading int not implemented for bitness " + bits.ToString() );
		}

		public static ulong PeekUInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.PeekUInt8();
				case BitUtils.Bitness.B16: return s.PeekUInt16().FromEndian( endian );
				case BitUtils.Bitness.B32: return s.PeekUInt32().FromEndian( endian );
				case BitUtils.Bitness.B64: return s.PeekUInt64().FromEndian( endian );
			}
			throw new Exception( "Peeking uint not implemented for bitness " + bits.ToString() );
		}

		public static long PeekInt( this Stream s, BitUtils.Bitness bits, EndianUtils.Endianness endian ) {
			switch ( bits ) {
				case BitUtils.Bitness.B8: return s.PeekInt8();
				case BitUtils.Bitness.B16: return s.PeekInt16().FromEndian( endian );
				case BitUtils.Bitness.B32: return s.PeekInt32().FromEndian( endian );
				case BitUtils.Bitness.B64: return s.PeekInt64().FromEndian( endian );
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
	}
}
