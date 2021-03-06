﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils {
	public static class HexUtils {
		public static byte ParseDecOrHexToByte( string s ) {
			s = s.Trim();

			if ( s.StartsWith( "0x" ) ) {
				s = s.Substring( 2 );
				return byte.Parse( s, System.Globalization.NumberStyles.HexNumber );
			} else {
				return byte.Parse( s );
			}
		}

		public static uint ParseDecOrHex( string s ) {
			s = s.Trim();

			if ( s.StartsWith( "0x" ) ) {
				s = s.Substring( 2 );
				return uint.Parse( s, System.Globalization.NumberStyles.HexNumber );
			} else {
				return uint.Parse( s );
			}
		}

		public static long ParseDecOrHexInt64(string s) {
			s = s.Trim();

			if (s.StartsWith("0x")) {
				s = s.Substring(2);
				return long.Parse(s, System.Globalization.NumberStyles.HexNumber);
			} else {
				return long.Parse(s);
			}
		}

		public static ulong ParseDecOrHexUInt64(string s) {
			s = s.Trim();

			if (s.StartsWith("0x")) {
				s = s.Substring(2);
				return ulong.Parse(s, System.Globalization.NumberStyles.HexNumber);
			} else {
				return ulong.Parse(s);
			}
		}

		public static byte[] HumanReadableHexStringToByteArray( string v ) {
			byte[] output;
			int mainloopstart;
			int outputlocation;
			if ( v.Length % 2 == 0 ) {
				output = new byte[v.Length / 2];
				mainloopstart = 0;
				outputlocation = 0;
			} else {
				// assume implicit leading zero
				output = new byte[( v.Length + 1 ) / 2];
				mainloopstart = 1;
				outputlocation = 1;
				output[0] = (byte) ParseHexNibble( v[0] );
			}

			for ( int i = mainloopstart; i < v.Length; i += 2 ) {
				int a = ParseHexNibble( v[i] );
				int b = ParseHexNibble( v[i + 1] );
				output[outputlocation] = (byte)( ( a << 4 ) | b );
				++outputlocation;
			}

			return output;
		}

		public static int ParseHexNibble( char c ) {
			switch ( c ) {
				case '0': return 0x0;
				case '1': return 0x1;
				case '2': return 0x2;
				case '3': return 0x3;
				case '4': return 0x4;
				case '5': return 0x5;
				case '6': return 0x6;
				case '7': return 0x7;
				case '8': return 0x8;
				case '9': return 0x9;
				case 'a': return 0xa;
				case 'b': return 0xb;
				case 'c': return 0xc;
				case 'd': return 0xd;
				case 'e': return 0xe;
				case 'f': return 0xf;
				case 'A': return 0xA;
				case 'B': return 0xB;
				case 'C': return 0xC;
				case 'D': return 0xD;
				case 'E': return 0xE;
				case 'F': return 0xF;
				default: throw new Exception( "'" + c + "' is not a valid hexadecimal digit." );
			}
		}
	}
}
