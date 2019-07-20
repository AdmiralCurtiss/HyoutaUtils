﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils {
	public static class TimeUtils {
		public static DateTime UnixTimeToDateTime( ulong unixTime ) {
			return new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc ).AddSeconds( unixTime ).ToLocalTime();
		}

		public static ulong DateTimeToUnixTime( DateTime time ) {
			return (ulong)( time - new DateTime( 1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc ).ToLocalTime() ).TotalSeconds;
		}
	}
}
