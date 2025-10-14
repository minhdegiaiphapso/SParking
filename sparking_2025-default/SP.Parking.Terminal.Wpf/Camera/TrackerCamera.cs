using Cirrious.CrossCore.Core;
using Cirrious.CrossCore;
using Green.Devices.Dal;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.Utility;
using SP.Parking.Terminal.Wpf.UI;
using SP.Parking.Terminal.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using GS.Camera.Wrapper.AvailableSource;

using GS.Camera.Wrapper;
namespace SP.Parking.Terminal.Wpf.Camera
{
	public class TrackerCamera : ICamera
	{
		private bool _isZoomFactorChanged = false;
		public bool IsZoomFactorChanged
		{
			get { return _isZoomFactorChanged; }
			set
			{
				_isZoomFactorChanged = value;
				ZoomReceived(new ZoomEventArgs { ZoomFactor = this.ZoomFactor });
			}
		}
		#region Tracker 2022Sep
		private TrackerFactor _trackerFactor;
		public TrackerFactor TrackerFactor
		{
			get
			{
				return _trackerFactor;
			}
			set
			{
				_trackerFactor = value;
			}
		}
		private TrackerEvent _trackerEvent;
		public TrackerEvent TrackerEvent
		{
			get
			{
				if (_trackerEvent == null)
					_trackerEvent = new TrackerEvent();

				return _trackerEvent;
			}
			set
			{
				_trackerEvent = value;
			}
		}
		public bool BlueTriggerStatus => player != null ? player.BlueFired : false;

		public bool RedTriggerStatus => player != null ? player.RedFired : false;
		private void ActiveTracker()
		{
			if (player != null)
			{
				player.SetTracker(this.TrackerFactor.XBegin,
					this.TrackerFactor.YBegin,
					this.TrackerFactor.WBegin,
					this.TrackerFactor.HBegin,
					this.TrackerFactor.BDBegin,
					this.TrackerFactor.XEnd,
					this.TrackerFactor.YEnd,
					this.TrackerFactor.WEnd,
					this.TrackerFactor.HEnd,
					this.TrackerFactor.BDEnd,
					this.TrackerFactor.FPS,
					backgroundName(this.TrackerFactor.TrackerType));
			}
		}
		private void OnRedFire(object sender, Tuple<DateTime, double> e)
		{
			_trackerEvent?.OnHasRed(e.Item1, e.Item2);
		}

		private void OnBlueFire(object sender, Tuple<DateTime, double> e)
		{
			_trackerEvent?.OnHasBlue(e.Item1, e.Item2);
		}
		private void OnBlueLost(object sender, Tuple<DateTime, double> e)
		{
			_trackerEvent?.OnLostBlue(e.Item1, e.Item2);
		}

		private void OnRedLost(object sender, Tuple<DateTime, double> e)
		{
			_trackerEvent?.OnLostRed(e.Item1, e.Item2);
		}
		//private void OnANPRResult(object sender, ResponseProcess e)
		//{
		//	//if (e != null)
		//	//{
		//	//	GS.Devices.Dal.ANPRResult res = new GS.Devices.Dal.ANPRResult() { Key = e.Key, Message = e.Message };
		//	//	if (e.Result != null)
		//	//	{
		//	//		var sr = e.Result.ToShortResult();
		//	//		res.CarAmount = sr.CarAmount;
		//	//		res.MotobikeAmount = sr.MotobikeAmount;
		//	//		res.CarPlate = sr.CarPlates;
		//	//		res.MotobikePlate = sr.MotobikePlates;
		//	//		res.TotalTimes = e.Result.TimeForward;
		//	//		_trackerEvent?.OnAnprResult(res);
		//	//	}
		//	//}
		//}
		#endregion
		private ZoomFactor _zoomFactor;
		public ZoomFactor ZoomFactor
		{
			get
			{
				if (_zoomFactor == null)
					_zoomFactor = new ZoomFactor();

				return _zoomFactor;
			}
			set
			{
				_zoomFactor = value;
			}
		}

		public System.Windows.Controls.UserControl Container { get; set; }

		public CameraOverlay Overlay { get; private set; }

		string _ipAddress;
		public string IPAddress
		{
			get { return _ipAddress; }
			set
			{
				if (_ipAddress == value)
					return;
				_ipAddress = value;

			}
		}
		string _userName;
		public string UserName
		{
			get { return _userName; }
			set
			{
				if (_userName == value)
					return;
				_userName = value;

			}
		}
		string _password;
		public string Password
		{
			get { return _password; }
			set
			{
				if (_password == value)
					return;
				_password = value;

			}
		}
		string _port;
		public string Port
		{
			get { return _port; }
			set
			{
				if (_port == value)
					return;
				_port = value;

			}
		}


		public TrackerCamera()
		{

		}
		public TrackerCamera(string ip)
		{

		}
		CarmeraView player;
		private IntPtr _playWndHandle = IntPtr.Zero;
		public void AddWndHandle(IntPtr wndHandle)
		{
			_playWndHandle = wndHandle;
		}
		System.Windows.Forms.Panel panel;
		string _waytype;
		public string WayType
		{
			get { return _waytype; }
			set
			{
				if (_waytype == value)
					return;
				_waytype = value;
				//if (DeviceInfo != null)
				//    DeviceInfo. = _password;
			}
		}
		int _channel;
		public int Channel
		{
			get { return _channel; }
			set
			{
				if (_channel == value)
					return;
				_channel = value;
				//if (DeviceInfo != null)
				//    DeviceInfo.Channel = Channel;
			}
		}
		public void Setup(string ip, string port, string username, string password, string waytype, int channel)
		{
			Container = new UserControl();
			Container.SizeChanged += Container_SizeChanged;
			panel = new System.Windows.Forms.Panel();
			panel.Location = new System.Drawing.Point(0, 0);
			panel.Dock = System.Windows.Forms.DockStyle.Fill;
			if (player == null)
				player = new CarmeraView();
			player.BlueOccurHandler += OnBlueFire;
			player.RedOccurHandler += OnRedFire;
			player.BlueLostHandler += OnBlueLost;
			player.RedLostHandler += OnRedLost;
			//player.AnprResultHandler += OnANPRResult;
			ElementHost host1 = new ElementHost();
			WindowsFormsHost host = new WindowsFormsHost();
			host1.Dock = System.Windows.Forms.DockStyle.Fill;
			host1.Child = player;
			panel.Controls.Add(host1);
			host.Child = panel;
			Container.Content = host;
			_ipAddress = ip;
			_port = port;
			_userName = username;
			_password = password;
			_channel = channel;
			_waytype = waytype;
			AddWndHandle(panel.Handle);
		}



		private void Container_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
		{
			if (player != null)
			{
				player.Width = Container.ActualWidth;
				player.Height = Container.ActualHeight;
			}
		}

		private Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
		{
			Bitmap result = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.DrawImage(bmp, 0, 0, width, height);
			}

			return result;
		}
		void SetOverlay(string content)
		{
			if (this.Overlay != null)
			{
				Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
				{
					this.Overlay.TextContent = content;
				});
			}
		}
		public void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel)
		{
			if (this.player == null)
			{
				Setup(ip, port, username, password, waytype, channel);
				if (zoomFactor != null)
					this.ZoomFactor = zoomFactor;
				else
					this.ZoomFactor = new ZoomFactor();
				if (trackerFactor != null)
					this.TrackerFactor = trackerFactor;
				else
					this.TrackerFactor = new TrackerFactor();

			}
			ActiveZoom();
			ActiveTracker();
			mapPlay(ip, port, username, password);
		}

		private string backgroundName(string type)
		{
			string originBackground = this.TrackerFactor != null && !string.IsNullOrEmpty(this.TrackerFactor.BackgroundName) ? this.TrackerFactor.BackgroundName : "";
			switch (type)
			{
				case "Vivotek":
					return $"{originBackground}_vk";
				case "Hik":
					return $"{originBackground}_hk";
				case "Rtsp":
					return $"{originBackground}_rp";
				case "RtspHd":
					return $"{originBackground}_rd";
				default:
					return $"{originBackground}_dt";
			}
		}
		private void mapPlay(string link, string port, string userName, string password)
		{
			if (player != null && this.TrackerFactor != null)
			{
				this.TrackerFactor.BackgroundName = this.TrackerEvent.BackgroundName;
				switch (this.TrackerFactor.TrackerType)
				{
					case "Vivotek":
						player.CallPlay(AvailableType.Viotek, link, port, userName, password, this.backgroundName("Vivotek"));
						return;
					case "Hik":
						player.CallPlay(AvailableType.Hik, link, port, userName, password, this.backgroundName("Hik"));
						return;
					case "Rtsp":
						player.CallPlay(AvailableType.Rtsp, link, port, userName, password, this.backgroundName("Rtsp"));
						return;
					case "RtspHd":
						player.CallPlay(AvailableType.RtspHD, link, port, userName, password, this.backgroundName("RtspHd"));
						return;
					default:
						break;
				}
			}
		}
		private void ReloadCamera(string ip, string port, string username, string password)
		{
			ActiveZoom();
			ActiveTracker();
			mapPlay(ip, port, username, password);
		}

		public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel)
		{
			if (this.player == null)
			{
				Setup(ip, port, username, password, waytype, channel);
			}
			ActiveZoom();
			ActiveTracker();
			mapPlay(ip, port, username, password);
		}
		public int DeviceId { get; set; }

		public event FrameEventHandler OnFrameReceived;

		public event ZoomEventHandler OnZoomReceived;

		public void FrameReceived(FrameEventArgs arg)
		{
			FrameEventHandler handler = OnFrameReceived;

			if (handler != null)
				handler(this, arg);
		}

		public void ZoomReceived(ZoomEventArgs arg)
		{
			ZoomEventHandler handler = OnZoomReceived;

			if (handler != null && _isZoomFactorChanged)
			{
				_isZoomFactorChanged = false;
				handler(this, arg);
			}
		}

		/// <summary>
		/// Starts capturing and sending image data.
		/// </summary>
		public void Start()
		{
			LoadZoomState(this.ZoomFactor);
			Connect();
			ActiveZoom();
			ActiveTracker();
		}

		/// <summary>
		/// Capture current frame image
		/// </summary>
		/// <returns></returns>
		public byte[] CaptureImage(string watermark = "")
		{
			try
			{
				byte[] bytes = CaptureZoom(watermark);
				//SaveZoomState();
				return bytes;
			}
			catch (Exception ex)
			{
				Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
				return null;
			}
		}

		public System.Drawing.Image CaptureImage()
		{
			var img = GetCameraBitMap();
			return (System.Drawing.Image)img;
		}

		public Bitmap GetCameraBitMap()
		{
			return player?.CaptureImage();
		}

		public byte[] CaptureZoom(string waterMark = "")
		{
			Bitmap bitMap = null;
			lock (this)
			{
				try
				{
					bitMap = GetCameraBitMap();
					if (bitMap == null) return null;

					ImageUtility.Watermark(bitMap, waterMark);
					return bitMap.ToByteArray(ImageFormat.Jpeg);
				}
				catch (Exception ex)
				{
					Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
					return null;
				}
				finally
				{
					if (bitMap != null)
					{
						bitMap.Dispose();
						bitMap = null;
					}
				}
			}
		}

		public void SaveZoomState()
		{
			try
			{
				if (this.ZoomFactor == null)
					this.ZoomFactor = new ZoomFactor();
				ReZoom();
				saveTracker();
			}
			catch (Exception ex)
			{
				Mvx.Resolve<ILogService>().Log(new Exception("Save zoom exception: " + ex.ToString()));
			}
		}
		private void saveTracker()
		{
			if (this.TrackerFactor == null)
				this.TrackerFactor = new TrackerFactor();
			if (player != null)
			{
				this.TrackerFactor.XBegin = player.LeftBegin;
				this.TrackerFactor.YBegin = player.TopBegin;
				this.TrackerFactor.WBegin = player.WidthBegin;
				this.TrackerFactor.HBegin = player.HeightBegin;
				this.TrackerFactor.XEnd = player.LeftEnd;
				this.TrackerFactor.YEnd = player.TopEnd;
				this.TrackerFactor.WEnd = player.WidthEnd;
				this.TrackerFactor.HEnd = player.HeightEnd;
				this.TrackerFactor.BDBegin = player.BoundDisplaymentBegin;
				this.TrackerFactor.BDEnd = player.BoundDisplaymentEnd;
				this.TrackerFactor.FPS = player.FPS;
				this.TrackerFactor.BackgroundName = player.BackGroundName;
			}
		}
		private void ReZoom()
		{
			if (player != null)
			{
				this.ZoomFactor.Factor = player.ZoomPercent;
				this.ZoomFactor.ZoomX = (float)player.XPosition;
				this.ZoomFactor.ZoomY = (float)player.YPosition;
			}
		}

		public void LoadZoomState(ZoomFactor factor)
		{
			try
			{

				this.ZoomFactor = factor;
				ActiveZoom();
			}
			catch (Exception ex)
			{
				Mvx.Resolve<ILogService>().Log(new Exception("Load zoom exception: " + ex.ToString()));
			}
		}

		private void ActiveZoom()
		{
			if (player != null)
			{
				player.SetZoom((int)this.ZoomFactor.Factor, this.ZoomFactor.ZoomX, this.ZoomFactor.ZoomY);
			}
		}

		public void Pause()
		{
			Disconnect();
		}

		private void Disconnect()
		{
			if (player != null)
				player.CallStop();
		}

		public void Continue()
		{
			Connect();
			ActiveZoom();
			ActiveTracker();
		}

		private void Connect()
		{
			if (player != null)
				mapPlay(this.IPAddress, this.Port, this.UserName, this.Password);
		}

		public void Stop()
		{
			Disconnect();
		}

		public void Dispose()
		{
			Disconnect();
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
		[DllImport("user32.dll")]
		static extern IntPtr GetWindowDC(IntPtr hWnd);
		[DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
		[DllImport("user32.dll")]
		static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

		public void ActiveZoom(bool active)
		{
			if (player != null)
				player.ShowConfig(active);
		}
	}
}
