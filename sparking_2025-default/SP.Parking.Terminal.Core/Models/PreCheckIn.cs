using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SP.Parking.Terminal.Core.Models
{
    public class CheckVehicleNumber
    {
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }

        [JsonProperty("card_id")]
        public string CardId { get; set; }

        [JsonProperty("customer_info")]
        public CustomerInfo CustomerInfo { get; set; }
    }
}
