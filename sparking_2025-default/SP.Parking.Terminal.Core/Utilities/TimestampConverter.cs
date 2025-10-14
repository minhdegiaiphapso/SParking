using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core
{
    public class TimestampConverter
    {
        private static DateTime milestone = new DateTime(2014, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime Timestamp2DateTime(long timestamp)
        {
            return milestone.AddSeconds(timestamp).ToLocalTime();
        }

        /*** Chinh sua 01-08-2016  ***/
        /// <summary>
        /// dd/MM/yyyy HH:mm:ss
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string Timestamp2String(long timestamp)
        {
            if (timestamp == -1)
                return string.Empty;

            return milestone.AddSeconds(timestamp).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static long DateTime2Timestamp(DateTime time)
        {
            //long k = (long)(time.ToUniversalTime() - milestone).TotalSeconds;
            return (long)(time.ToUniversalTime() - milestone).TotalSeconds;
        }

        public static long DateTime2TimestampOrigin(DateTime time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return (long)(time.ToUniversalTime() - dtDateTime).TotalSeconds;
        }
    }
}