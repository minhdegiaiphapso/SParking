using System;
using Cirrious.MvvmCross.Localization;

namespace SP.Parking.Terminal.Core
{
    public static class DateTimeExtensions
	{
        public static long ToUnixTimestamp( this DateTime dt )
		{
            var epoc = new DateTime(1970, 1, 1);
            var delta = dt - epoc;
            if (delta.TotalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException("Unix epoc starts January 1st, 1970");
            }
            return (long) delta.TotalSeconds;
		}

        public static DateTime FromUnixTimestamp( long unixTime )
        {
            var date = new DateTime(1970, 1, 1);
            date = date.AddSeconds(unixTime);
            return date;
        }
	}
}

