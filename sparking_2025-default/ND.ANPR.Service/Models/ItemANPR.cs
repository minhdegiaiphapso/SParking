using Newtonsoft.Json;
using System.Collections.Generic;

namespace ND.ANPR.Service.Models
{
	public class WaterClockItem
	{
		[JsonProperty("ResultDetect")]
		public string ResultDetect { get; set; }
	}
	public class ItemANPR
	{
		[JsonProperty("MotobikeLP")]
		public IList<PlateItem> MotobikeLP { get; set; }
		[JsonProperty("CarLP")]
		public IList<PlateItem> CarLP { get; set; }
		[JsonProperty("Objects")]
		public ANPRObjectItem Objects { get; set; }
		[JsonProperty("ProcessTime")]
		public string ProcessTime { get; set; }
		public string CarVehicleNumberSelected { 
			get
			{
				if (CarLP != null && CarLP.Count > 0)
					return CarLP[0].VehicleNumber;
				return string.Empty;
			} 
		}
		public string CarVehicleNumberPaddingSelected
		{
			get
			{
				if (CarLP != null && CarLP.Count > 0)
					return CarLP[0].VehicleNumberPadding;
				return string.Empty;
			}
		}
		public string BikeVehicleNumberSelected
		{
			get
			{
				if (MotobikeLP != null && MotobikeLP.Count > 0)
					return MotobikeLP[0].VehicleNumber;
				return string.Empty;
			}
		}
		public string BikeVehicleNumberPaddingSelected
		{
			get
			{
				if (MotobikeLP != null && MotobikeLP.Count > 0)
					return MotobikeLP[0].VehicleNumberPadding;
				return string.Empty;
			}
		}
	}
	public class AngleItem
	{
		[JsonProperty("Angle")]
		public double Angle { get; set; }
		[JsonProperty("Direction")]
		public string Direction { get; set; }
		[JsonProperty("Estimate")]
		public string Estimate { get; set; }
	}
	public class PlateItem
	{
		[JsonProperty("VehicleNumber")]
		public string VehicleNumber { get; set; }
		[JsonProperty("VehicleNumberPadding")]
		public string VehicleNumberPadding { get; set; }
		[JsonProperty("Bound")]
		public string Bound { get; set; }
		[JsonProperty("Angle")]
		public AngleItem AngleItem { get; set; }
		[JsonProperty("Bounds")]
		public IList<string> Bounds { get; set; }
	}
	public class CounterObjectItem
	{
		[JsonProperty("Amount")]
		public int Amount { get; set; }
		[JsonProperty("Bounds")]
		public IList<string> Bounds { get; set; }
	}
	public class ANPRObjectItem
	{
		[JsonProperty("Car")]
		public CounterObjectItem Car { get; set; }
		[JsonProperty("CarPlate")]
		public CounterObjectItem CarPlate { get; set; }
		[JsonProperty("Motobike")]
		public CounterObjectItem Motobike { get; set; }
		[JsonProperty("MotobikePlate")]
		public CounterObjectItem MotobikePlate { get; set; }
	}
}
