using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public class AvailableCard
	{
		[JsonProperty("id")]
		public long Id { get; set; }
		[JsonProperty("card_id")]
		public string CardId { get; set; }

		[JsonProperty("vehicle_number")]
		public string VehicleNumber { get; set; }
	}
}
