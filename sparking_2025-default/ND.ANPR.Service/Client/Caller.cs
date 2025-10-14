using ND.ANPR.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ND.ANPR.Service.Client
{
	public class Caller
	{
		APICaller myCaller;
		public Caller()
		{
			myCaller = new APICaller();
		}
	
		public void WaterClock(byte[] image, Action<WaterClockItem, Exception> complete)
		{
			myCaller.WaterClock(image, complete);
		}
		public void WaterClock(string fileImage , Action<WaterClockItem, Exception> complete)
		{
			myCaller.WaterClock(fileImage, complete);
		}

	
		public void Anpr(byte[] image, Action<ItemANPR, Exception> complete)
		{
			myCaller.Anpr(image, complete);
			
		}
		public void Anpr(string fileImage, Action<ItemANPR, Exception> complete)
		{
			myCaller.Anpr(fileImage, complete);
		}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberByCar(byte[] image)
		//{
		//	var res = await myCaller.Anpr(image);
		//	return new Tuple<string, Exception>(res.Item1!=null?res.Item1.CarVehicleNumberSelected:string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberByCar(byte[] image, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberByCar(image);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberByBike(byte[] image)
		//{
		//	var res = await myCaller.Anpr(image);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.BikeVehicleNumberSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberByBike(byte[] image, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberByBike(image);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberByCar(string fileImage)
		//{
		//	var res = await myCaller.Anpr(fileImage);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.CarVehicleNumberSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberByCar(string fileImage, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberByCar(fileImage);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberByBike(string fileImage)
		//{
		//	var res = await myCaller.Anpr(fileImage);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.BikeVehicleNumberSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberByBike(string fileImage, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberByBike(fileImage);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberPaddingByCar(byte[] image)
		//{
		//	var res = await myCaller.Anpr(image);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.CarVehicleNumberPaddingSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberPaddingByCar(byte[] image, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberPaddingByCar(image);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberPaddingByBike(byte[] image)
		//{
		//	var res = await myCaller.Anpr(image);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.BikeVehicleNumberPaddingSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberPaddingByBike(byte[] image, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberPaddingByBike(image);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberPaddingByCar(string fileImage)
		//{
		//	var res = await myCaller.Anpr(fileImage);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.CarVehicleNumberPaddingSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberPaddingByCar(string fileImage, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberPaddingByCar(fileImage);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
		//public async Task<Tuple<string, Exception>> FindVehicelNumberPaddingByBike(string fileImage)
		//{
		//	var res = await myCaller.Anpr(fileImage);
		//	return new Tuple<string, Exception>(res.Item1 != null ? res.Item1.BikeVehicleNumberPaddingSelected : string.Empty, res.Item2);
		//}
		//public void FindVehicelNumberPaddingByBike(string fileImage, Action<string, Exception> complete)
		//{
		//	if (complete != null)
		//	{
		//		Task.Factory.StartNew(async () => {
		//			var res = await this.FindVehicelNumberPaddingByBike(fileImage);
		//			complete(res.Item1, res.Item2);
		//		});
		//	}
		//}
	}
}
