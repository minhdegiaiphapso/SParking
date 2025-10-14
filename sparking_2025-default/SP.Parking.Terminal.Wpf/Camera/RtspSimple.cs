using Cirrious.CrossCore.Core;
using Cirrious.CrossCore;
using Green.Devices.Dal;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.Utility;
using SP.Parking.Terminal.Wpf.RtspSupport;
using SP.Parking.Terminal.Wpf.UI;
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


namespace SP.Parking.Terminal.Wpf.Camera
{
	public class RtspSimple : ICamera
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

		#region 2022Oct11 Camera Tracker
		public TrackerFactor TrackerFactor { get; set; }
		public TrackerEvent TrackerEvent { get; set; }
		public bool BlueTriggerStatus => false;

		public bool RedTriggerStatus => false;
		#endregion
		public RtspSimple()
		{

		}
		public RtspSimple(string ip)
		{

		}
		RtspPlayer player;
		private IntPtr _playWndHandle = IntPtr.Zero;
		public void AddWndHandle(IntPtr wndHandle)
		{
			_playWndHandle = wndHandle;
		}
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
		System.Windows.Forms.Panel panel;
		public void Setup(string ip, string port, string username, string password, string waytype, int channel)
		{
			Container = new UserControl();
			Container.SizeChanged += Container_SizeChanged;
			panel = new System.Windows.Forms.Panel();
			panel.Location = new System.Drawing.Point(0, 0);
			panel.Dock = System.Windows.Forms.DockStyle.Fill;
			if (player == null)
				player = new RtspPlayer();
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
			_waytype = waytype;
			_channel = channel;
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

			}
			ActiveZoom();
			player.CallPlay(ip, username, password);
		}

		private void ReloadCamera(string ip, string port, string username, string password)
		{
			ActiveZoom();
			player.CallPlay(ip, username, password);

		}

		public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel)
		{
			if (this.player == null)
			{
				Setup(ip, port, username, password, waytype, channel);
			}
			ActiveZoom();
			player.CallPlay(ip, username, password);
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
			//return player.Capture();
			Bitmap bmp = null;
			lock (this)
			{
				try
				{
					IntPtr pt = IntPtr.Zero;

					Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
					{
						pt = panel.Handle;
					});

					//Bitmap bmp = null;
					IntPtr bmpDC;
					//Graphics g;
					IntPtr cameraDC;
					RECT windowRect = new RECT(0, 0, 0, 0);
					User32.GetWindowRect(pt, ref windowRect);

					cameraDC = User32.GetWindowDC(pt);

					if (cameraDC != IntPtr.Zero)
					{
						bmp = new Bitmap(windowRect.Width, windowRect.Height);

						using (Graphics g = System.Drawing.Graphics.FromImage(bmp))
						{
							bmpDC = g.GetHdc();
							GDI32.BitBlt(bmpDC, 0, 0, windowRect.Width, windowRect.Height, cameraDC, 0, 0, TernaryRasterOperations.SRCCOPY);
							//GDI32.BitBlt(bmpDC, cw, ch, (int)(windowRect.Width/ft), (int)(windowRect.Height/ft), cameraDC, 0, 0, TernaryRasterOperations.SRCCOPY);
							g.ReleaseHdc(bmpDC);

							User32.ReleaseDC(pt, cameraDC);
						}
						return bmp;
						//return clone(bmp);
					}

					return null;
				}
				catch (Exception ex)
				{
					Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
					return null;
				}
				finally
				{
					if (bmp != null)
					{
						//bmp.Dispose();
						//bmp = null;
					}
				}
			}
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
			}
			catch (Exception ex)
			{
				Mvx.Resolve<ILogService>().Log(new Exception("Save zoom exception: " + ex.ToString()));
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
		}

		private void Connect()
		{
			if (player != null)
				player.CallPlay(this.IPAddress, this.UserName, this.Password);
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
