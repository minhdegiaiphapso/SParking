using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Controls;

/// <summary>
/// 
/// </summary>
namespace Green.Devices.Dal
{
    public class ZoomFactor
    {
        public float Factor { get; set; }
        public float ZoomX { get; set; }
        public float ZoomY { get; set; }
        public bool ZoomEnabled { get; set; }
        public int BoxWidth { get; set; }
        public int BoxHeight { get; set; }

        public ZoomFactor()
        {
            Factor = 100;
            BoxWidth = 200;
            BoxHeight = 200;
        }
    }

	/// <summary>
	/// Describes an event relating to new image frames. 
	/// </summary>
	public class FrameEventArgs
	{
		/// <summary>
		/// Gets or set the image frame.
		/// </summary>
		public Bitmap Frame { get; set; }
	}

    public class ZoomEventArgs
    {
        public ZoomFactor ZoomFactor { get; set; }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The <see cref="FrameEventArgs"/> instance containing the event data.</param>
	public delegate void FrameEventHandler(object sender, FrameEventArgs e);
    public delegate void ZoomEventHandler(object sender, ZoomEventArgs e);


	public class TrackerEvent
	{
		public bool WhenTheSame { get; set; } = false;
		public bool BlueInUse { get; set; } = false;
		public bool RedInUse { get; set; } = false;
		public string BackgroundName { get; set; } = "";
		public event EventHandler<Tuple<DateTime, double>> BlueOccurHandler;
		public event EventHandler<Tuple<DateTime, double>> RedOccurHandler;
		public event EventHandler<Tuple<DateTime, double>> BlueLostHandler;
		public event EventHandler<Tuple<DateTime, double>> RedLostHandler;
		public event EventHandler<ANPRResult> PlateHandler;
		public void OnHasBlue(DateTime date, double boundReplacement)
		{
			if (BlueInUse)
				BlueOccurHandler?.Invoke(this, new Tuple<DateTime, double>(date, boundReplacement));
		}
		public void OnHasRed(DateTime date, double boundReplacement)
		{
			if (RedInUse)
				RedOccurHandler?.Invoke(this, new Tuple<DateTime, double>(date, boundReplacement));
		}
		public void OnLostBlue(DateTime date, double boundReplacement)
		{
			if (BlueInUse)
				BlueLostHandler?.Invoke(this, new Tuple<DateTime, double>(date, boundReplacement));
		}
		public void OnLostRed(DateTime date, double boundReplacement)
		{
			if (RedInUse)
				RedLostHandler?.Invoke(this, new Tuple<DateTime, double>(date, boundReplacement));
		}
		public void OnAnprResult(ANPRResult res)
		{
			PlateHandler?.Invoke(this, res);
		}
	}
	public class ANPRResult
	{
		public string Key { get; set; }
		public string Message { get; set; }
		public int CarAmount { get; set; }
		public int MotobikeAmount { get; set; }
		public List<string> CarPlate { get; set; }
		public List<string> MotobikePlate { get; set; }
		public long TotalTimes { get; set; }
	}
	public class TrackerFactor
	{
		public double XBegin { get; set; } = 0;
		public double YBegin { get; set; } = 0;
		public double WBegin { get; set; } = 0;
		public double HBegin { get; set; } = 0;
		public int BDBegin { get; set; } = 1;
		public double XEnd { get; set; } = 0;
		public double YEnd { get; set; } = 0;
		public double WEnd { get; set; } = 0;
		public double HEnd { get; set; } = 0;
		public int BDEnd { get; set; } = 1;
		public string BackgroundName { get; set; }
		public int FPS { get; set; } = 5;
		public string TrackerType { get; set; }
		public TrackerFactor()
		{
		}
	}
	/// <summary>
	/// Describes a capture device.
	/// </summary>
	/// 
	public interface ICamera
	{
        int DeviceId { get; set; }
        string IPAddress { get;  set; }
        string Port { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string WayType { get; set; }
        int Channel { get; set; }
		bool BlueTriggerStatus { get; }
		bool RedTriggerStatus { get; }
		TrackerFactor TrackerFactor { get; set; }
		TrackerEvent TrackerEvent { get; set; }
		ZoomFactor ZoomFactor { get; set; }
        void ActiveZoom(bool active);
        /// <summary>
        /// Occurs when a new frame is received.
        /// </summary>
        event FrameEventHandler OnFrameReceived;

        event ZoomEventHandler OnZoomReceived;

        void FrameReceived(FrameEventArgs arg);

		/// <summary>
		/// Starts capturing and sending image data.
		/// </summary>
		void Start();

		/// <summary>
		/// Pauses the device temporarily.
		/// </summary>
		void Pause();

        /// <summary>
        /// Continue capturing device
        /// </summary>
        void Continue();
        
		/// <summary>
		/// Stops the device and closes it. After calling this function, the capture devices
		/// cannot be started again.
		/// </summary>
		void Stop();

        void Dispose();

        /// <summary>
        /// Capture current frame image
        /// </summary>
        /// <returns></returns>
        byte[] CaptureImage(string waterMark = "");

        System.Drawing.Image CaptureImage();

        /// <summary>
        /// Setups the specified ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        void Setup(string ip, string port, string username, string password, string waytype, int channel);
        //void Load(string ip, ZoomFactor zoomFactor);
        void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel);

        /// <summary>
        /// Saves the state of the zoom.
        /// </summary>
        void SaveZoomState();

        void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel);

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        UserControl Container { get; set; }
	}
}
