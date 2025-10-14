using Newtonsoft.Json;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public enum LaneDirection
    {
        Unknown = 9999,
        In = 0,
        Out,
    }

    public class Lane
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("vehicle_type")]
        //public VehicleType VehicleType { get; set; }

        [JsonProperty("vehicle_type")]
        public int VehicleTypeId { get; set; }

        [JsonProperty("direction")]
        public LaneDirection Direction { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("terminal_id")]
        public int TerminalId { get; set; }
        //[JsonProperty("FlagIn")]
        //public bool FlagIn { get; set; }
        //[JsonProperty("FlagOut")]
        //public bool FlafOut { get; set; }
    }
}
