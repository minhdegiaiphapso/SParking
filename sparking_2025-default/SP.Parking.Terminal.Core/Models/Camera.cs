using Cirrious.CrossCore;
using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Services;
using Green.Devices.Dal;
using System;
using System.Windows.Controls;

namespace SP.Parking.Terminal.Core.Models
{
    /// <summary>
    /// The camera position
    /// </summary>
    public enum CameraPosition
    {
        Front = 0,
        Back = 1,
        Extra1 = 2,
        Extra2 = 3
    }

    /// <summary>
    /// Camera model
    /// </summary>
    public class Camera :IDisposable
    {
        /// <summary>
        /// Gets or sets the identifier of camera
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of camera
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address of camera
        /// </summary>
        [JsonProperty("ip")]
        public string IP { get; set; }
        /// <summary>
        /// Gets or sets the Port of camera
        /// </summary>
        [JsonProperty("port")]
        public string Port { get; set; }
        /// <summary>
        /// Gets or sets the UserName of camera
        /// </summary>
        [JsonProperty("username")]
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the Password of camera
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("waytype")]
        public string WayType { get; set; }
        /// <summary>
        /// Gets or sets the CameraType of camera
        /// </summary>
        [JsonProperty("cameratype")]
        public CameraType CameraType { get; set; }
        /// <summary>
        /// Gets or sets the position of camera
        /// </summary>
        [JsonProperty("position")]
        public CameraPosition Position { get; set; }

        /// <summary>
        /// Gets of sets the direction of camera
        /// </summary>
        [JsonProperty("direction")]
        public LaneDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the serial number of camera
        /// </summary>
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("channel")]
        public int Channel { get; set; }

        /// <summary>
        /// Gets or sets the lane id owner of camera
        /// </summary>
        [JsonIgnore]
        public int LaneId { get; set; }

        //[JsonIgnore]
        //private ZoomFactor _zoomFactor;
        [JsonProperty("zoom_factor")]
        public ZoomFactor ZoomFactor { get; set; }
		[JsonProperty("tracker_factor")]
		public TrackerFactor TrackerFactor { get; set; }
		[JsonIgnore]
		public TrackerEvent TrackerEvent { get; set; }
		[JsonIgnore]
        public ICamera RawCamera { get; set; }
        [JsonIgnore]
        public bool Zoomable { get; set; }

        /// <summary>
        /// Occurs when a new frame is received.
        /// </summary>
        event FrameEventHandler OnFrameReceived;

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
            if (handler != null)
                handler(this, arg);
        }

        /// <summary>
        /// Starts capturing and sending image data.
        /// </summary>
        public void Start()
        {
            if (this.RawCamera != null)
                this.RawCamera.Start();
        }

        /// <summary>
        /// Pauses the device temporarily.
        /// </summary>
        public void Pause()
        {
            if (this.RawCamera != null)
                this.RawCamera.Pause();
        }

        /// <summary>
        /// Continue capturing device
        /// </summary>
        public void Continue()
        {
            if (this.RawCamera != null)
                this.RawCamera.Continue();
        }

        /// <summary>
        /// Stops the device and closes it. After calling this function, the capture devices
        /// cannot be started again.
        /// </summary>
        public void Stop()
        {
            if (this.RawCamera != null)
                this.RawCamera.Stop();
            //this.ZoomFactor = this.RawCamera.ZoomFactor;
        }

        public void SaveStates()
        {
            if (this.RawCamera != null)
                this.RawCamera.SaveZoomState();
        }

        /// <summary>
        /// Capture current frame image
        /// </summary>
        /// <returns></returns>
        public byte[] CaptureImage(string waterMark)
        {
            try
            {
                if (this.RawCamera != null)
                {
                    int count = 3;
                    byte[] img = RawCamera.CaptureImage(waterMark);
                    while (count > 0 && img == null)
                    {
                        System.Threading.Thread.Sleep(100);
                        img = RawCamera.CaptureImage(waterMark);
                        count--;
                    }
                    return img;
                }
                return null;
            }
            catch(Exception ex)
            {
                Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
                return null;
            }
        }

        public System.Drawing.Image CaptureImage()
        {
            try
            {
                if (this.RawCamera != null)
                {
                    int count = 3;
                    System.Drawing.Image img = RawCamera.CaptureImage();
                    while (count > 0 && img == null)
                    {
                        System.Threading.Thread.Sleep(100);
                        img = RawCamera.CaptureImage();
                        count--;
                    }
                    return img;
                }
                return null;
            }
            catch (Exception ex)
            {
                Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
                return null;
            }
        }

        /// <summary>
        /// Setups the specified ip.
        /// </summary>
        /// <param name="ip">The ip.</param>
        //public void Setup(string ip)
        //{
        //    if (this.RawCamera != null)
        //        this.RawCamera.Setup(ip);
        //    //this.RawCamera.OnFrameReceived += Camera_OnFrameReceived;
        //}

        public void Setup(CameraType type, bool zoomable, bool cameraTypeChanged)
        {
            if (RawCamera == null)
                RawCamera = Mvx.Resolve<ICamera>();
            else if (cameraTypeChanged)
                RawCamera = Mvx.Resolve<ICamera>();
			//RawCamera.IPAddress = this.IP;
			//RawCamera.Port = this.Port;
			//RawCamera.UserName = this.UserName;
			//RawCamera.Password = this.Password;
			//this.RawCamera.Load(this.IP, this.ZoomFactor);
			setTrackerType();
			this.RawCamera.TrackerEvent = this.TrackerEvent;
			this.RawCamera.Load(this.IP, this.Port, this.UserName, this.Password, zoomable, this.ZoomFactor, this.TrackerFactor, this.WayType=this.Direction.ToString(), this.Channel);
            this.RawCamera.OnZoomReceived += RawCamera_OnZoomReceived;
            //this.RawCamera.OnFrameReceived += Camera_OnFrameReceived;
        }
		private void setTrackerType()
		{
			if (TrackerFactor == null)
				TrackerFactor = new TrackerFactor();
			switch (CameraType)
			{
				case CameraType.VivotekTracker:
					this.TrackerFactor.TrackerType = "Vivotek";
					break;
				case CameraType.HikTracker:
					this.TrackerFactor.TrackerType = "Hik";
					break;
				case CameraType.RTSPTracker:
					this.TrackerFactor.TrackerType = "Rtsp";
					break;
				case CameraType.RTSPHDTracker:
					this.TrackerFactor.TrackerType = "RtspHd";
					break;
				default:
					this.TrackerFactor.TrackerType = "Old";
					break;
			}

		}
		void RawCamera_OnZoomReceived(object sender, ZoomEventArgs e)
        {
            this.ZoomFactor = e.ZoomFactor;
            this.ZoomReceived(e);
        }

        void Camera_OnFrameReceived(object sender, FrameEventArgs e)
        {
            this.FrameReceived(e);
        }

        public void Dispose()
        {
            try
            {
                this.RawCamera.Stop();
            }
            catch (Exception ex)
            {
                // TODO: Add log file
            }
            this.RawCamera.Dispose();
        }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        [JsonIgnore]
        public UserControl Container { get { return this.RawCamera.Container; } }

        public Camera()
        {
			this.TrackerEvent = new TrackerEvent();
			this.TrackerFactor = new TrackerFactor();
		}
    }
}
