using System;
using System.Text;
using System.Text.RegularExpressions;

namespace HyoutaUtils {
	public static class TextUtils {
		public enum GameTextEncoding {
			ASCII,
			ShiftJIS,
			UTF8,
			UTF16,
		}

		private static Encoding _ShiftJISEncoding = null;
		public static Encoding ShiftJISEncoding {
			get {
				if ( _ShiftJISEncoding == null ) {
					_ShiftJISEncoding = Encoding.GetEncoding( 932 );
				}
				return _ShiftJISEncoding;
			}
		}

		public static string GetTextShiftJis( byte[] file, int location ) {
			if ( location == -1 ) return null;

			try {
				int i = location;
				while ( file[i] != 0x00 ) {
					i++;
				}
				string Text = ShiftJISEncoding.GetString( file, location, i - location );
				return Text;
			} catch ( Exception ) {
				return null;
			}
		}

		public static string GetTextAscii( byte[] file, int location ) {
			if ( location == -1 ) return null;

			try {
				int i = location;
				while ( file[i] != 0x00 ) {
					i++;
				}
				string Text = Encoding.ASCII.GetString( file, location, i - location );
				return Text;
			} catch ( Exception ) {
				return null;
			}
		}

		public static string GetTextUnicode( byte[] file, int location, int maxByteLength ) {
			StringBuilder sb = new StringBuilder();
			for ( int i = 0; i < maxByteLength; i += 2 ) {
				ushort ch = BitConverter.ToUInt16( file, location + i );
				if ( ch == 0 || ch == 0xFFFF ) { break; }
				sb.Append( (char)ch );
			}
			return sb.ToString();
		}

		public static string GetTextUTF8( byte[] file, int location ) {
			int tmp;
			return GetTextUTF8( file, location, out tmp );
		}

		public static string GetTextUTF8( byte[] file, int location, out int nullLocation ) {
			if ( location == -1 ) { nullLocation = -1; return null; }

			try {
				int i = location;
				while ( file[i] != 0x00 ) {
					i++;
				}
				string Text = Encoding.UTF8.GetString( file, location, i - location );
				nullLocation = i;
				return Text;
			} catch ( Exception ) {
				nullLocation = -1;
				return null;
			}
		}

		public static string TrimNull( this string s ) {
			int n = s.IndexOf( '\0', 0 );
			if ( n >= 0 ) {
				return s.Substring( 0, n );
			}
			return s;
		}

		public static byte[] StringToBytesShiftJis( string s ) {
			//byte[] bytes = ShiftJISEncoding.GetBytes(s);
			//return bytes.TakeWhile(subject => subject != 0x00).ToArray();
			return ShiftJISEncoding.GetBytes( s );
		}

		public static byte[] StringToBytesUTF16( string s ) {
			return Encoding.Unicode.GetBytes( s );
		}

		public static string XmlEscape( string s ) {
			s = s.Replace( "&", "&amp;" );
			s = s.Replace( "\"", "&quot;" );
			s = s.Replace( "'", "&apos;" );
			s = s.Replace( "<", "&lt;" );
			s = s.Replace( ">", "&gt;" );
			return s;
		}

		public static string Truncate( this string value, int maxLength ) {
			if ( string.IsNullOrEmpty( value ) ) return value;
			return value.Length <= maxLength ? value : value.Substring( 0, maxLength );
		}
	}
}