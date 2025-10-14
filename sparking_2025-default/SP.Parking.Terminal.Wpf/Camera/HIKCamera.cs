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

namespace SP.Parking.Terminal.Wpf.Devices
{
    public enum DISPLAY_MODE
    {
        Callback = 0,
        Direct = 1
    }

    public class DeviceInfo
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Channel { get; set; }
    }

    public class HIKCamera : ICamera
    {
        public DeviceInfo DeviceInfo { get; set; }

        private int _handle;
        private IntPtr _playWndHandle = IntPtr.Zero;
        private int _port;

        private bool _isPaused;
        public bool IsPaused { get { return _isPaused; } }

        private bool _isStarted;

        private static bool _sdkInit = false;

        private byte[] rgbData = null;

        private PlayCtrl.DISPLAYCBFUN _func;
        private CHCNetSDK.REALDATACALLBACK _realDataCallbackFunc;

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
                //ChangeIPAddress(_ipAddress, _portname, _userName, _password);
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
                    DeviceInfo.Password = Password;
            }
        }
        string _portname;
        public string Port
        {
            get { return _portname; }
            set
            {
                if (_portname == value)
                    return;
                _portname = value;
                //if (DeviceInfo != null)
                //    DeviceInfo.Port = _portname;
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
                //    DeviceInfo.Port = _portname;
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
                if (DeviceInfo != null)
                    DeviceInfo.Channel = Channel;
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

        /// <summary>
        /// Starts capturing and sending image data.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Start()
        {
            if (!_isStarted)
            {
                StartDeviceWithMode(DISPLAY_MODE.Direct);
                TryResize();
                ReZoom();
            }
        }

        /// <summary>
        /// Add window handle to display device's image
        /// </summary>
        /// <param name="wndHandle"></param>
        public void AddWndHandle(IntPtr wndHandle)
        {
            //_playWndHandle = wndHandle;
        }

        /// <summary>
        /// Play device
        /// </summary>
        private bool StartDeviceWithMode(DISPLAY_MODE mode)
        {
            CHCNetSDK.NET_DVR_CLIENTINFO lpClientInfo = new CHCNetSDK.NET_DVR_CLIENTINFO();
            lpClientInfo.lChannel = DeviceInfo?.Channel ?? 1; 
            lpClientInfo.lLinkMode = 1;
            lpClientInfo.sMultiCastIP = null;

            int handle = -1;
            _playWndHandle = PicBox.Handle;
            if (mode == DISPLAY_MODE.Callback)
            {
                lpClientInfo.hPlayWnd = IntPtr.Zero; 
                 _func = new PlayCtrl.DISPLAYCBFUN(RemoteDisplayCBFun);

                _realDataCallbackFunc = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);
                IntPtr pUser = new IntPtr();
                handle = CHCNetSDK.NET_DVR_RealPlay_V30(DeviceId, ref lpClientInfo, _realDataCallbackFunc, pUser, 1);
               
            }
            else if (mode == DISPLAY_MODE.Direct)
            {
              
                lpClientInfo.hPlayWnd = _playWndHandle;
                IntPtr pUser = new IntPtr();
                handle = CHCNetSDK.NET_DVR_RealPlay_V30(DeviceId, ref lpClientInfo, null, pUser, 0);
            }

            if (handle > -1)
            {
                _handle = handle;
                _isStarted = true;
                //_playWndHandle = PicBox.Handle;
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Convert YV12 byte array to RGB byte array
        /// </summary>
        public unsafe static void ConvertYUVtoRGB(byte* data, int width, int height, ref byte[] newData)
        {
            int size = width * height;
            int offset = size;

            if (newData == null)
                newData = new byte[size * 3];

            int u, v, y1, y2, y3, y4;

            for (int i = 0, k = 0; i < size; i += 2, k += 1)
            {
                y1 = data[i];
                y2 = data[i + 1];
                y3 = data[width + i];
                y4 = data[width + i + 1];

                u = data[offset + k];
                v = data[offset + (size / 4) + k];

                int[] pixel0 = ConvertYUVtoRGB(y1, u, v);
                newData[i * 3] = (byte)pixel0[0];
                newData[i * 3 + 1] = (byte)pixel0[1];
                newData[i * 3 + 2] = (byte)pixel0[2];


                int[] pixel1 = ConvertYUVtoRGB(y2, u, v);
                newData[(i + 1) * 3] = (byte)pixel1[0];
                newData[(i + 1) * 3 + 1] = (byte)pixel1[1];
                newData[(i + 1) * 3 + 2] = (byte)pixel1[2];

                int[] pixel2 = ConvertYUVtoRGB(y3, u, v);
                newData[(width + i) * 3] = (byte)pixel2[0];
                newData[(width + i) * 3 + 1] = (byte)pixel2[1];
                newData[(width + i) * 3 + 2] = (byte)pixel2[2];

                int[] pixel3 = ConvertYUVtoRGB(y4, u, v);
                newData[(width + i + 1) * 3] = (byte)pixel3[0];
                newData[(width + i + 1) * 3 + 1] = (byte)pixel3[1];
                newData[(width + i + 1) * 3 + 2] = (byte)pixel3[2];

                if (i != 0 && (i + 2) % width == 0)
                    i += width;
            }

            //return newData;
        }

        /// <summary>
        /// Calculate RGB value from YUV value
        /// </summary>
        private static int[] ConvertYUVtoRGB(int y, int u, int v)
        {
            int c, d, e;
            c = y - 16;
            d = u - 128;
            e = v - 128;

            int r = (298 * c + 409 * e + 128) >> 8;
            int g = (298 * c - 100 * d - 208 * e + 128) >> 8;
            int b = (298 * c + 516 * d + 128) >> 8;

            r = (r > 255 ? 255 : r < 0 ? 0 : r);
            g = (g > 255 ? 255 : g < 0 ? 0 : g);
            b = (b > 255 ? 255 : b < 0 ? 0 : b);

            int[] result = new int[3];
            result[0] = r;
            result[1] = g;
            result[2] = b;
            return result;
        }

        /// <summary>
        /// Device's per-frame callback
        /// </summary>
        public unsafe void RemoteDisplayCBFun(int port, IntPtr buff, int size, int width, int height, int stamp, int type, int reserved)
        {
            //byte[] managedArray = new byte[size];
            //Marshal.Copy(buff, managedArray, 0, size);
            byte* managedArray = (byte*)buff;

            ConvertYUVtoRGB(managedArray, width, height, ref rgbData);

            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                BitmapData bmpD = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                IntPtr ptr = bmpD.Scan0;
                int bytes = bmpD.Stride * bmp.Height;
                Marshal.Copy(rgbData, 0, ptr, bytes);
                bmp.UnlockBits(bmpD);
                //bmp.Save(@"D:\adafs.bmp");
                FrameReceived(new FrameEventArgs() { Frame = bmp });
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public BitmapSource LoadBitmap(Bitmap bmp)
        {
            var pBmp = bmp.GetHbitmap();

            BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(pBmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(pBmp);

            return bmpSrc;
        }

        Bitmap _bmp = null;
        protected void FrameReceived(FrameEventArgs arg)
        {
            try
            {


                // TODO: Get per-frame bitmap here
                if (_bmp == null)
                    _bmp = new Bitmap(arg.Frame.Width, arg.Frame.Height, arg.Frame.PixelFormat);

                BitmapData srcBmpData = arg.Frame.LockBits(new Rectangle(0, 0, arg.Frame.Width, arg.Frame.Height), ImageLockMode.WriteOnly, arg.Frame.PixelFormat);
                BitmapData desBmpData = _bmp.LockBits(new Rectangle(0, 0, _bmp.Width, _bmp.Height), ImageLockMode.WriteOnly, _bmp.PixelFormat);

                IntPtr ptrSrc = srcBmpData.Scan0;
                IntPtr ptrDes = desBmpData.Scan0;

                int bytes = srcBmpData.Stride * _bmp.Height;

                byte[] data = new byte[bytes];
                Marshal.Copy(ptrSrc, data, 0, bytes);
                Marshal.Copy(data, 0, ptrDes, bytes);

                _bmp.UnlockBits(desBmpData);
                arg.Frame.UnlockBits(srcBmpData);

                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                //    new Action(() => imgView.Source = LoadBitmap(_bmp)));

                //picBox.Image = _bmp;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            FrameEventHandler handler = OnFrameReceived;

            if (handler != null)
                handler(this, arg);
        }

        /// <summary>
        /// A callback when playing device real-time
        /// </summary>
        /// <param name="realHandle"></param>
        /// <param name="dataType"></param>
        /// <param name="buffer"></param>
        /// <param name="bufSize"></param>
        /// <param name="user"></param>
        private void RealDataCallBack(int realHandle, uint dataType, ref byte buffer, uint bufSize, IntPtr user)
        {
            if (_isPaused)
                return;

            switch (dataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    int tPort = -1;
                    if (!PlayCtrl.PlayM4_GetPort(ref tPort))
                    {
                        Debug.WriteLine("Get port fail");
                    }
                    _port = tPort;

                    if (bufSize > 0)
                    {
                        //set as stream mode, real-time stream under preview
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(_port, PlayCtrl.STREAME_REALTIME))
                        {
                            Debug.WriteLine("PlayM4_SetStreamOpenMode fail");
                        }
                        //start player
                        if (!PlayCtrl.PlayM4_OpenStream(_port, ref buffer, bufSize, 1024 * 1024))
                        {
                            _port = -1;
                            Debug.WriteLine("PlayM4_OpenStream fail");
                            break;
                        }
                        //set soft decode display callback function to capture

                        if (!PlayCtrl.PlayM4_SetDisplayCallBack(_port, _func))
                        {
                            Debug.WriteLine("PlayM4_SetDisplayCallBack fail");
                        }

                        //start play, set play window
                        Debug.WriteLine("About to call PlayM4_Play");

                        //if (_playWndHandle != IntPtr.Zero)
                        //{

                        if (!PlayCtrl.PlayM4_Play(_port, _playWndHandle))
                        {
                            _port = -1;
                            Debug.WriteLine("PlayM4_Play fail");
                            break;
                        }
                        //}
                        //else
                        //{
                        //    Debug.WriteLine("PlayM4_Play fail - Missing window handle to display");
                        //}

                        //set frame buffer number

                        if (!PlayCtrl.PlayM4_SetDisplayBuf(_port, 15))
                        {
                            Debug.WriteLine("PlayM4_SetDisplayBuf fail");
                        }

                        //set display mode
                        if (!PlayCtrl.PlayM4_SetOverlayMode(_port, 0, 0/* COLORREF(0)*/))//play off screen // todo!!!
                        {
                            Debug.WriteLine("PlayM4_SetOverlayMode fail ");
                        }
                    }

                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (bufSize > 0 && _port != -1)
                    {
                        if (!PlayCtrl.PlayM4_InputData(_port, ref buffer, bufSize))
                        {
                            Debug.WriteLine("PlayM4_InputData fail ");
                        }
                    }
                    break;

                case CHCNetSDK.NET_DVR_AUDIOSTREAMDATA:     //  Audio Stream Data
                    if (bufSize > 0 && _port != -1)
                    {
                        if (!PlayCtrl.PlayM4_InputVideoData(_port, ref buffer, bufSize))
                        {
                            Debug.WriteLine("PlayM4_InputVideoData Fail ");
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Pauses the device temporarily.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Continue playing device
        /// </summary>
        public void Continue()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Stops the device and closes it. After calling this function, the capture devices
        /// cannot be started again.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Stop()
        {
            Debug.WriteLine("Stop device - " + DeviceId);
            if (!CHCNetSDK.NET_DVR_StopRealPlay(_handle))
                Debug.WriteLine("Stop device: " + DeviceId + " fail");         

            _isStarted = false;
        }

        /// <summary>
        /// Release SDK resource
        /// </summary>
        public void Dispose()
        {
            if (_sdkInit)
            {
                CHCNetSDK.NET_DVR_Cleanup();
                _sdkInit = false;
            }
        }

        public int DeviceId { get; set; }

        public ZoomFactor ZoomFactor { get; set; }

        public event FrameEventHandler OnFrameReceived;

        public event ZoomEventHandler OnZoomReceived;

        void ICamera.FrameReceived(FrameEventArgs arg)
        {

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

        public System.Drawing.Image CaptureImage()
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
                        pt = ParentBox.Handle;
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
        #region Box
        public PictureBox PicBox { get; set; }
        public PictureBox ParentBox { get; set; }
        public System.Windows.Forms.Panel Pnl { get; set; }
        System.Windows.Forms.Button btn;
        public void ActiveZoom(bool active)
        {
            if (active)
                btn.Visible = true;
            else
            {
                btn.Visible = false;
                Pnl.Visible = false;
            }
        }
        private System.Windows.Forms.Label lbl;
        private System.Windows.Forms.ToolTip toolTip1;
        private void TryResize()
        {
            try
            {
                System.Windows.Forms.Panel pnl = ParentBox.Parent as System.Windows.Forms.Panel;
                foreach (var control in pnl.Controls)
                {
                    if (control is System.Windows.Forms.Panel)
                    {
                        System.Windows.Forms.Panel lbl = control as System.Windows.Forms.Panel;
                        if (ParentBox.Width != pnl.Width)
                            ParentBox.Width = pnl.Width;
                        if (ParentBox.Height != pnl.Height - lbl.Height)
                            ParentBox.Height = pnl.Height - lbl.Height;
                        if (PicBox.Width < ParentBox.Width)
                            PicBox.Width = ParentBox.Width;
                        if (PicBox.Height < ParentBox.Height)
                            PicBox.Height = ParentBox.Height;
                    }
                }
            }
            catch
            {
                ;
            }
        }
        private void Panel_Resize(object sender, EventArgs e)
        {
            TryResize();
            ReZoom();
        }
        private void BtnRight_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Zoomx = this.ZoomFactor.ZoomX - factor;

                this.ZoomFactor.ZoomX = Zoomx;
                ReZoom();
            }
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Zoomx = this.ZoomFactor.ZoomY - factor;

                this.ZoomFactor.ZoomY = Zoomx;
                ReZoom();
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Zoomx = this.ZoomFactor.ZoomY + factor;
                this.ZoomFactor.ZoomY = Zoomx;
                ReZoom();
            }
        }

        private void BtnLeft_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Zoomx = this.ZoomFactor.ZoomX + factor;
                this.ZoomFactor.ZoomX = Zoomx;
                ReZoom();
            }
        }
        private void BtnZoomout_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Factor = this.ZoomFactor.Factor + factor;
                float Zoomx = this.ZoomFactor.ZoomX + factor / 2;
                float Zoomy = this.ZoomFactor.ZoomY + factor / 2;
                if (Factor <= 400)
                {
                    this.ZoomFactor.Factor = Factor;
                    this.ZoomFactor.ZoomX = Zoomx;
                    this.ZoomFactor.ZoomY = Zoomy;
                    ReZoom();
                }

            }
        }

        private void ReZoom()
        {
            if (this.ZoomFactor != null)
            {
                float Factor = this.ZoomFactor.Factor;
                float ScaleW = (float)ParentBox.Width / (float)this.ZoomFactor.BoxWidth;
                float ScaleH = (float)ParentBox.Height / (float)this.ZoomFactor.BoxHeight;
                float Zoomx = this.ZoomFactor.ZoomX * ScaleW;
                float Zoomy = this.ZoomFactor.ZoomY * ScaleH;
                if (Factor > 100 && Factor <= 400)
                {
                    int pw = ParentBox.Width;
                    int ph = ParentBox.Height;
                    int tw = (int)(pw * (Factor / 100));
                    int th = (int)(ph * (Factor / 100));
                    float maxleft = tw - pw;
                    float maxtop = th - ph;
                    if (Zoomx > maxleft)
                    {
                        Zoomx = maxleft;
                    }
                    if (Zoomy > maxtop)
                    {
                        Zoomy = maxtop;
                    }
                    if (Zoomx < 0)
                        Zoomx = 0;
                    if (Zoomy < 0)
                        Zoomy = 0;
                    this.ZoomFactor.ZoomX = Zoomx;
                    this.ZoomFactor.ZoomY = Zoomy;
                    this.ZoomFactor.BoxWidth = pw;
                    this.ZoomFactor.BoxHeight = ph;
                    PicBox.Left = (int)(-Zoomx);
                    PicBox.Top = (int)(-Zoomy);
                    PicBox.Width = tw;
                    PicBox.Height = th;
                }
                else if (Factor > 400)
                {
                    int pw = ParentBox.Width;
                    int ph = ParentBox.Height;
                    int tw = 4 * pw;
                    int th = 4 * ph;
                    PicBox.Width = 4 * pw;
                    PicBox.Height = 4 * ph;
                    float maxleft = tw - pw;
                    float maxtop = th - ph;
                    if (Zoomx > maxleft)
                    {
                        Zoomx = maxleft;
                    }
                    if (Zoomy > maxtop)
                    {
                        Zoomy = maxtop;
                    }
                    if (Zoomx < 0)
                        Zoomx = 0;
                    if (Zoomy < 0)
                        Zoomy = 0;
                    this.ZoomFactor.ZoomX = Zoomx;
                    this.ZoomFactor.ZoomY = Zoomy;
                    this.ZoomFactor.BoxWidth = pw;
                    this.ZoomFactor.BoxHeight = ph;
                    PicBox.Left = (int)(-Zoomx);
                    PicBox.Top = (int)(-Zoomy);
                    PicBox.Width = tw;
                    PicBox.Height = th;
                }
                else
                {
                    int pw = ParentBox.Width;
                    int ph = ParentBox.Height;
                    this.ZoomFactor.ZoomX = 0;
                    this.ZoomFactor.ZoomY = 0;
                    this.ZoomFactor.BoxWidth = pw;
                    this.ZoomFactor.BoxHeight = ph;
                    PicBox.Left = 0;
                    PicBox.Top = 0;
                    PicBox.Width = pw;
                    PicBox.Height = ph;
                }
                toolTip1.ToolTipTitle = "{ Factor: " + this.ZoomFactor.Factor + "%, Left: " + ((int)this.ZoomFactor.ZoomX).ToString() +
                    ", Top:" + ((int)this.ZoomFactor.ZoomY).ToString() + ", Width:" + ((int)this.ZoomFactor.BoxWidth).ToString() + ", Height:" + ((int)this.ZoomFactor.BoxHeight).ToString() + "}";
            }
        }
        private void BtnZoomin_Click(object sender, EventArgs e)
        {
            if (this.ZoomFactor != null)
            {
                int factor = 5;
                float Factor = this.ZoomFactor.Factor - factor;
                float Zoomx = this.ZoomFactor.ZoomX - factor / 2;
                float Zoomy = this.ZoomFactor.ZoomY - factor / 2;
                if (Factor >= 100)
                {
                    this.ZoomFactor.Factor = Factor;

                    this.ZoomFactor.ZoomX = Zoomx;

                    this.ZoomFactor.ZoomX = Zoomy;
                    ReZoom();
                }

            }
        }
        private void Btn_Click(object sender, EventArgs e)
        {
            TryResize();
            if (Pnl.Visible)
            {
                Pnl.Visible = false;

            }
            else
            {
                Pnl.Visible = true;
            }
        }
        #endregion
        public void Setup(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            if (!_sdkInit)
                _sdkInit = CHCNetSDK.NET_DVR_Init();

            Container = new System.Windows.Controls.UserControl();
            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
            
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            PicBox = new PictureBox();
            PicBox.SizeMode = PictureBoxSizeMode.Zoom;
            PicBox.BackColor = Color.Black;
            PicBox.Location = new System.Drawing.Point(0, 0);
            //PicBox.Dock = DockStyle.Fill;
            WindowsFormsHost host = new WindowsFormsHost();
            ParentBox = new PictureBox();
            ParentBox.BackColor = Color.AliceBlue;
            ParentBox.SizeMode = PictureBoxSizeMode.Zoom;
            ParentBox.Location= new System.Drawing.Point(0, 0);
            ParentBox.Controls.Add(PicBox);

            Pnl = new System.Windows.Forms.Panel();
            Pnl.Visible = false;

            System.Windows.Forms.Button btnZoomin = new System.Windows.Forms.Button();

            btnZoomin.FlatStyle = FlatStyle.Flat;
            btnZoomin.FlatAppearance.BorderSize = 0;
            btnZoomin.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnZoomin.ForeColor = System.Drawing.Color.White;
            btnZoomin.BackgroundImage = Properties.Resources.zoom_out;
            btnZoomin.BackgroundImageLayout = ImageLayout.Zoom;

            btnZoomin.Width = 25;
            btnZoomin.Height = 25;
            btnZoomin.Left = 120;

            btnZoomin.Click += BtnZoomin_Click;
            System.Windows.Forms.Button btnZoomout = new System.Windows.Forms.Button();

            btnZoomout.FlatStyle = FlatStyle.Flat;
            btnZoomout.FlatAppearance.BorderSize = 0;
            btnZoomout.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnZoomout.ForeColor = System.Drawing.Color.White;
            btnZoomout.BackgroundImage = Properties.Resources.zoom_in;
            btnZoomout.BackgroundImageLayout = ImageLayout.Zoom;

            btnZoomout.Width = 25;
            btnZoomout.Height = 25;
            btnZoomout.Left = btnZoomin.Left + btnZoomin.Width + 5;

            btnZoomout.Click += BtnZoomout_Click;

            System.Windows.Forms.Button btnLeft = new System.Windows.Forms.Button();
            btnLeft.FlatStyle = FlatStyle.Flat;
            btnLeft.FlatAppearance.BorderSize = 0;
            btnLeft.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnLeft.ForeColor = System.Drawing.Color.White;

            btnLeft.Width = 25;
            btnLeft.Height = 25;
            btnLeft.Left = btnZoomout.Left + btnZoomout.Width + 25;

            btnLeft.Text = "<";
            btnLeft.Click += BtnLeft_Click;

            System.Windows.Forms.Button btnUp = new System.Windows.Forms.Button();
            btnUp.FlatStyle = FlatStyle.Flat;
            btnUp.FlatAppearance.BorderSize = 0;
            btnUp.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnUp.ForeColor = System.Drawing.Color.White;

            btnUp.Width = 25;
            btnUp.Height = 25;
            btnUp.Left = btnLeft.Left + btnZoomout.Width + 5;
            btnUp.Text = "^";

            btnUp.Click += BtnUp_Click;
            System.Windows.Forms.Button btnDown = new System.Windows.Forms.Button();
            btnDown.FlatStyle = FlatStyle.Flat;
            btnDown.FlatAppearance.BorderSize = 0;
            btnDown.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnDown.ForeColor = System.Drawing.Color.White;

            btnDown.Width = 25;
            btnDown.Height = 25;
            btnDown.Left = btnUp.Left + btnZoomout.Width + 5;
            btnDown.Text = "v";
            btnDown.Click += BtnDown_Click;

            System.Windows.Forms.Button btnRight = new System.Windows.Forms.Button();
            btnRight.FlatStyle = FlatStyle.Flat;
            btnRight.FlatAppearance.BorderSize = 0;
            btnRight.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btnRight.ForeColor = System.Drawing.Color.White;

            btnRight.Width = 25;
            btnRight.Height = 25;
            btnRight.Left = btnDown.Left + btnZoomout.Width + 5;
            btnRight.Text = ">";

            btnRight.Click += BtnRight_Click;
            Pnl.Controls.Add(btnZoomout);
            Pnl.Controls.Add(btnZoomin);
            Pnl.Controls.Add(btnLeft);
            Pnl.Controls.Add(btnUp);
            Pnl.Controls.Add(btnDown);
            Pnl.Controls.Add(btnRight);
            lbl = new System.Windows.Forms.Label();

            lbl.BackColor = System.Drawing.Color.Transparent;
            lbl.ForeColor = System.Drawing.Color.White;
            lbl.Text = ip;
            toolTip1 = new System.Windows.Forms.ToolTip();

            // Set up the delays for the ToolTip.
            //toolTip1.AutoPopDelay = 500;
            //toolTip1.InitialDelay = 500;
            //toolTip1.ReshowDelay = 500;
            toolTip1.SetToolTip(lbl, "IP Address: " + ip);
            btn = new System.Windows.Forms.Button();

            btn.Width = 30;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
            btn.ForeColor = System.Drawing.Color.White;
            btn.BackgroundImage = Properties.Resources.config_camera;
            btn.BackgroundImageLayout = ImageLayout.Zoom;

            btn.Dock = DockStyle.Right;
            btn.Click += Btn_Click;
            System.Windows.Forms.Panel subpnl = new System.Windows.Forms.Panel();

            subpnl.Dock = DockStyle.Bottom;
            subpnl.Height = 30;
            subpnl.BackColor = System.Drawing.Color.FromArgb(125, 55, 189, 176);
            lbl.Left = 5;

            Pnl.Dock = DockStyle.Bottom;
            Pnl.Height = subpnl.Height;

            lbl.Top = (subpnl.Height - lbl.Height) / 2 + 3;

            btnZoomin.Top = btnZoomout.Top = btnLeft.Top = btnRight.Top = btnUp.Top = btnDown.Top = (subpnl.Height - btnZoomin.Height) / 2;

            subpnl.Controls.Add(lbl);
            subpnl.Controls.Add(btn);
            subpnl.Controls.Add(Pnl);
            subpnl.Controls.SetChildIndex(Pnl, 999);
            subpnl.Controls.SetChildIndex(lbl, -1);
            subpnl.Controls.SetChildIndex(btn, 9);

            System.Windows.Forms.ContextMenu cm = new System.Windows.Forms.ContextMenu();
            panel.Resize += Panel_Resize;
            panel.Controls.Add(ParentBox);
            panel.Controls.Add(subpnl);
            panel.Controls.SetChildIndex(ParentBox, -1);
            panel.Controls.SetChildIndex(subpnl, 9);
            host.Child = panel;
           
            Container.Content = host;
            ParentBox.Width = panel.Width;
            ParentBox.Height = panel.Height - lbl.Height;
            AddWndHandle(PicBox.Handle);
            Login(ip, port, username, password, channel);
            Start();
            _ipAddress = ip;
            _portname = port;
            _userName = username;
            _password = password;
            _channel = channel;
        }

        private void Login(string ip, string port, string username, string password, int channel = 1)
        {
            //string username = "admin";
            //string password = "hd543211";
            ////string password = "@GP142536";
            string port1 = OtherUtilities.GetPort(ip);
            string host = ip;
            if (!string.IsNullOrEmpty(port1))
                host = OtherUtilities.RemoveLastNonDigitWordChar(ip.Replace(port1, ""));
            if (string.IsNullOrEmpty(port))
                port = "8000";
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 deviceV30 = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            DeviceId = CHCNetSDK.NET_DVR_Login_V30(host, int.Parse(port), username, password, ref deviceV30);

            if (DeviceId > -1)
            {
                DeviceInfo = new DeviceInfo() { IP = host, Port = int.Parse(port), UserName = username, Password = password, Channel = channel };
            }
        }

        private void Logout()
        {
            if (!CHCNetSDK.NET_DVR_Logout(DeviceId))
            {
                Debug.WriteLine("Logout device: " + DeviceId + " fail");
            }
        }
		public TrackerFactor TrackerFactor { get; set; }
		public TrackerEvent TrackerEvent { get; set; }

		public bool BlueTriggerStatus => false;

		public bool RedTriggerStatus => false;
		public void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel = 1)
        {
            if (DeviceInfo==null || ParentBox == null)
            {
                Setup(ip, port, username, password, waytype, channel);
            }
            else if (!DeviceInfo.IP.Equals(ip) || !DeviceInfo.Port.Equals(port)||!DeviceInfo.UserName.Equals(username)|| !DeviceInfo.Password.Equals(password))
            {
                _ipAddress = ip;
                _portname = port;
                _userName = username;
                _password = password;
                _channel = channel;
                Logout();
                Login(ip, port, username, password, channel);
            }
            if (zoomFactor != null)
                this.ZoomFactor = zoomFactor;
            else
                this.ZoomFactor = new ZoomFactor();
        }

        public void SaveZoomState()
        {

        }

        public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            if (DeviceInfo == null || ParentBox == null)
            {
                Setup(ip, port, username, password, waytype, channel);
            }
            else if (!DeviceInfo.IP.Equals(ip) || !DeviceInfo.Port.Equals(port) || !DeviceInfo.UserName.Equals(username) || !DeviceInfo.Password.Equals(password))
            {
                _ipAddress = ip;
                _portname = port;
                _userName = username;
                _password = password;
                _channel = channel;
                Logout();
                Login(ip, port, username, password, channel);
            }          
        }

        public System.Windows.Controls.UserControl Container { get; set; }
    }
}
