using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class ServerTimeInfo : MvxNotifyPropertyChanged
    {
        [JsonProperty("utc_time")]
        public DateTime UtcTime { get; set; }

        [JsonProperty("local_time")]
        public DateTime LocalTime { get; set; }
    }
}
