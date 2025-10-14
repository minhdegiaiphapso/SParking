using System;
using System.Collections.Generic;
using System.Text;

namespace SP.Parking.Terminal.Core
{
	/// <summary>
	/// Filter used to search an <see cref="IDirectoryService"/>.
	/// </summary>
    public class HttpUtility
	{
        public static string EscapeDataString(string data)
        {
            if( data.Length < 10000 )
                return Uri.EscapeDataString(data);

            int limit = 10000;

            StringBuilder sb = new StringBuilder();
            int loops = data.Length / limit;

            for (int i = 0; i <= loops; i++)
            {
                if (i < loops)
                {
                    sb.Append(Uri.EscapeDataString(data.Substring(limit * i, limit)));
                }
                else
                {
                    sb.Append(Uri.EscapeDataString(data.Substring(limit * i)));
                }
            }

            return sb.ToString();
        }
    }
}

