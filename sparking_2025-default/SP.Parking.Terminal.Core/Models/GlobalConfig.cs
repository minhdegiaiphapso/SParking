using Newtonsoft.Json;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class GlobalConfig
    {
        [JsonProperty("parking_name")]
        public string ParkingName { get; set; }

        [JsonProperty("log_server")]
        public string LogServer { get; set; }
    }
}
