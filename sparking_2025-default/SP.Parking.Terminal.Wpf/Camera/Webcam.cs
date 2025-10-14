using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Green.Devices.Dal;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using System.Drawing.Drawing2D;
using SP.Parking.Terminal.Core.Utility;
using Cirrious.CrossCore.Core;
using Green.APS.Devices.HikVision;
using SP.Parking.Terminal.Wpf.UI;
using SP.Parking.Terminal.Core.Utilities;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace SP.Parking.Terminal.Wpf.Devices
{
    public class MyImageBox: ImageBox
    {
        public MyImageBox()
        {
            PanableAndZoomable = false;
        }
    }

    public class Webcam : ICamera
    {
        public int DeviceId { get; set; }

        public ZoomFactor ZoomFactor { get; set; }

        public event FrameEventHandler OnFrameReceived;

        public event ZoomEventHandler OnZoomReceived;

        private VideoCapture _capture = null;

        public DeviceInfo DeviceInfo { get; set; }
        public void ActiveZoom(bool active)
        {; }
        MyImageBox PicBox { get; set; }

        public CameraOverlay Overlay { get; private set; }

        string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;

                //if (this.Overlay != null)
                //    this.Overlay.TextContent = _ipAddress;

               // SetOverlay(_ipAddress);

                if (DeviceInfo != null)
                    DeviceInfo.IP = _ipAddress;
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
                if (DeviceInfo != null)
                    DeviceInfo.UserName = _userName;
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
                if (DeviceInfo != null)
                    DeviceInfo.Password = _password;
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
                //if (DeviceInfo != null)
                //    DeviceInfo. = _password;
            }
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
        void SetOverlay(string content)
        {
            if (this.Overlay != null)
            {
                Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() => {
                    this.Overlay.TextContent = content;
                });
            }
        }

        public Webcam()
        {
			DeviceInfo = new DeviceInfo();
			_capture = new VideoCapture(0);
			_capture.ImageGrabbed += ProcessFrame;
		}

        public void FrameReceived(FrameEventArgs arg)
        {
        }

        public void Start()
        {
            _capture.Start();
        }

        public void Pause()
        {
        }

        public void Continue()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
            if (_capture != null)
                _capture.Dispose();
        }

        public byte[] CaptureImage(string waterMark = "")
        {
            try
            {
                byte[] bytes = CaptureZoom(waterMark);
                SaveZoomState();
                return bytes;
            }
            catch (Exception ex)
            {
                Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
                return null;
            }
        }

        public byte[] CaptureZoom(string waterMark = "")
        {
            Bitmap bmp = null;
            lock (this)
            {
                try
                {
                    bmp = GetCameraBitMap();
                    return bmp.ToByteArray(ImageFormat.Jpeg);
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
                        bmp.Dispose();
                        bmp = null;
                    }
                }
            }
        }

        public Image CaptureImage()
        {
            var img = GetCameraBitMap();
            return (System.Drawing.Image)img;
        }

        public Bitmap GetCameraBitMap()
        {
            Bitmap bmp = null;
            lock (this)
            {
                try
                {
                    IntPtr pt = IntPtr.Zero;

                    Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() => {
                        pt = PicBox.Handle;
                    });

                    IntPtr bmpDC;
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
                            g.ReleaseHdc(bmpDC);
                        }
                        User32.ReleaseDC(pt, cameraDC);
                        return bmp;
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

        private void ProcessFrame(object sender, EventArgs arg)
        {
			Mat m = new Mat();
			_capture.Retrieve(m);
			PicBox.Image = m;
		}

        public void Setup(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            this.Container = new System.Windows.Controls.UserControl();
            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;

            WindowsFormsHost wfhost = new WindowsFormsHost();
            PicBox = new MyImageBox();
            PicBox.SizeMode = PictureBoxSizeMode.Zoom;
            PicBox.HorizontalScrollBar.Enabled = false;
            PicBox.VerticalScrollBar.Enabled = false;
            PicBox.Location = new System.Drawing.Point(0, 0);
            PicBox.Dock = System.Windows.Forms.DockStyle.Fill;
            

            panel.Controls.Add(PicBox);
            wfhost.Child = panel;
            Container.Content = wfhost;

            Start();

            //Overlay = new CameraOverlay()
            //{
            //    Container = this.Container,
            //    TextContent = DeviceInfo.IP,
            //};

            IPAddress = ip;
            Port = port;
            UserName = username;
            Password = password;
        }
		public TrackerFactor TrackerFactor { get; set; }
		public TrackerEvent TrackerEvent { get; set; }

		public bool BlueTriggerStatus => false;

		public bool RedTriggerStatus => false;
		public void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel = 1)
        {
            if (PicBox == null)
            {
                Setup(ip, port, username, password, waytype);
            }
        }

        public void SaveZoomState()
        {
        }

        public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
        }

        public System.Windows.Controls.UserControl Container { get; set; }
    }
}
