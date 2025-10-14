using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ND.ANPR.Service.Models;
using System.IO;

using System.Threading;
using System.Numerics;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Headers;
using System.Web.UI.WebControls;

namespace ND.ANPR.Service
{
	public class APICaller
	{
		private HttpClient _httpClient;
		//RestSharp.RestClient _restClient;
		public APICaller()
		{
			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri("http://localhost:8192/");
			_httpClient.Timeout = TimeSpan.FromSeconds(3);
			//_restClient = new RestClient("http://localhost:8192/");
		}
		
		public async Task<bool> CheckHealth()
		{
			try
			{
				var response = await _httpClient.GetAsync("api/nd-health/");
				
				if (response != null && response.IsSuccessStatusCode) { 
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}
		public async void Anpr(byte[] image, Action<ItemANPR, Exception> complete)
		{
			try
			{
				using (var content = new MultipartFormDataContent())
				{
					//_httpClient.BaseAddress = new Uri("http://localhost:8192/");

					// Add string parameter
					content.Add(new StringContent("vehicle"), "mode_detect");

					// Add file parameter
					var fileContent = new ByteArrayContent(image);
					fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
					content.Add(fileContent, "source_thumb", "source_thumb.jpg");

					// Send POST request
					var response = await _httpClient.PostAsync("api/nd-anpr/", content);

					// Read response
					string json = await response.Content.ReadAsStringAsync();

					if (!response.IsSuccessStatusCode)
					{
						throw new Exception($"Server returned {(int)response.StatusCode}: {json}");
					}

					var result = JsonConvert.DeserializeObject<ItemANPR>(json);

					// Ensure callback on UI thread
					complete?.Invoke(result, null);
				}
			}
			catch (Exception ex) {
				complete?.Invoke(null, ex);
			}
			
		}
	
		public void WaterClock(string fileImg, Action<WaterClockItem, Exception> complete)
		{
			if (!File.Exists(fileImg))
			{
				if (complete != null)
					complete?.Invoke(null, new Exception("File ảnh không tồn tại!"));
				return;
			}
			var img = File.ReadAllBytes(fileImg);
			WaterClock(img, complete);	
		}
		public async void WaterClock(byte[] image, Action<WaterClockItem, Exception> complete)
		{
			try
			{
				using (var content = new MultipartFormDataContent())
				{
					//_httpClient.BaseAddress = new Uri("http://localhost:8192/");


					// Add file parameter
					var fileContent = new ByteArrayContent(image);
					fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
					content.Add(fileContent, "source_thumb", "source_thumb.jpg");

					// Send POST request
					var response = await _httpClient.PostAsync("api/water-clock/", content);

					// Read response
					string json = await response.Content.ReadAsStringAsync();

					if (!response.IsSuccessStatusCode)
					{
						throw new Exception($"Server returned {(int)response.StatusCode}: {json}");
					}

					var result = JsonConvert.DeserializeObject<WaterClockItem>(json);

					// Ensure callback on UI thread
					complete?.Invoke(result, null);
				}
			}
			catch (Exception ex) {
				complete?.Invoke(null, ex);
			}	
		}
		public void Anpr(string fileImg, Action<ItemANPR, Exception> complete)
		{
			if (!File.Exists(fileImg))
			{
				if (complete != null)
					complete?.Invoke(null, new Exception("File ảnh không tồn tại!"));
				return;
			}
			var img = File.ReadAllBytes(fileImg);
			Anpr(img, complete);
		}
		private async Task<Exception> GetException(HttpResponseMessage response)
		{
			if (response != null)
			{
				if (response.IsSuccessStatusCode)
				{
					switch (response.StatusCode)
					{
						case System.Net.HttpStatusCode.OK:
						case System.Net.HttpStatusCode.Created:
							return null;						
						default:
							var dfct = await response.Content.ReadAsStringAsync();
							return new Exception("Tham số không hợp lệ");
					}
				}
				else
				{
					var nsct = await response.Content.ReadAsStringAsync();
					return new Exception("Không thể thực hiện");
				}
			}
			else
			{
				return new Exception("Không thể kết nối đến server");
			}
		}
	}
}
