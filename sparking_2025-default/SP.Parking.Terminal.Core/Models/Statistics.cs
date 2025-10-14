using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class StatisticsItem
    {
        [JsonProperty("check_in")]
        public int CheckInNumber { get; set; }
        [JsonProperty("check_out")]
        public int CheckOutNumber { get; set; }
        [JsonProperty("remain")]
        public int Remain { get; set; }
    }

    public class Statistics
    {
        List<CardType> _cardTypes;
        [JsonProperty("card_types")]
        public List<CardType> CardTypes
        {
            get { return _cardTypes; }
            set
            {
                _cardTypes = value;
            }
        }

        List<VehicleType> _vehicleTypes;
        [JsonProperty("vehicle_types")]
        public List<VehicleType> VehicleType
        {
            get { return _vehicleTypes; }
            set
            {
                _vehicleTypes = value;
            }
        }

        
        Dictionary<string, Dictionary<string, StatisticsItem>> _data;
        [JsonProperty("data")]
        public Dictionary<string, Dictionary<string, StatisticsItem>> Data
        {
            get;
            set;
        }
    }
}
