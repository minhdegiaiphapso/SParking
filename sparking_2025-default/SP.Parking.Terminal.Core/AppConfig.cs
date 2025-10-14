using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core
{
    public class AppConfig
    {
        // Web API
        public static class WebApi
        {
            // Base url
            public static string RootUrl { get; set; }
            public static string BaseApiUrl { get; set; }           

            public static string LogUrl { get; private set; }

            public static string ClientId { get; private set; }            

            static WebApi()
            {
                //#if TESTING
                //RootUrl = "https://115.79.51.140:8001";
                //BaseApiUrl = "https://115.79.51.140:8001/v1";
                //#else
                RootUrl = "https://api.manga360.net";
                BaseApiUrl = "https://api.manga360.net/v1";
                //#endif
            }
        }
    }
}
