using AxVITAMINDECODERLib;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.Utility;
using SP.Parking.Terminal.Wpf.UI;
//using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Wpf.Views;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
//using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Media;
using VITAMINDECODERLib;

namespace SP.Parking.Terminal.Wpf.Devices
{
    #region old version
    /// <summary>
    /// 
    /// </summary>
    //public class VitaminCamera_old : ICamera
    //{
    //    private bool _isZoomFactorChanged = false;
    //    public bool IsZoomFactorChanged
    //    {
    //        get { return _isZoomFactorChanged; }
    //        set
    //        {
    //            _isZoomFactorChanged = value;
    //            ZoomReceived(new ZoomEventArgs { ZoomFactor = this.ZoomFactor });
    //        }
    //    }

    //    private ZoomFactor _zoomFactor;
    //    public ZoomFactor ZoomFactor
    //    {
    //        get
    //        {
    //            if (_zoomFactor == null)
    //                _zoomFactor = new ZoomFactor();

    //            return _zoomFactor;
    //        }
    //        set
    //        {
    //            _zoomFactor = value;
    //        }
    //    }

    //    public System.Windows.Controls.UserControl Container { get; set; }
    //    //public UserControl Container { get { return VitaminControl.Container; } }

    //    public AxVitaminCtrl VitaminControl { get; set; }

    //    public CameraOverlay Overlay { get; private set; }

    //    bool _canUse = false;
    //    bool _connectionOK = false;
    //    int _timeSinceConnectionOK = 0;

    //    string _ipAddress;
    //    public string IPAddress
    //    {
    //        get { return _ipAddress; }
    //        set
    //        {
    //            _ipAddress = value;

    //            //if (this.Overlay != null)
    //            //    this.Overlay.TextContent = _ipAddress;

    //            //SetOverlay(_ipAddress + " - " + this.ZoomFactor.Factor + "%");

    //            if (VitaminControl != null)
    //                VitaminControl.RemoteIPAddr = _ipAddress;
    //        }
    //    }

    //    //byte[] _data = null;

    //    public VitaminCamera_old()
    //    {

    //    }

    //    public VitaminCamera_old(string ip)
    //    {

    //    }

    //    public void Setup(string ip)
    //    {
    //        Container = new System.Windows.Controls.UserControl();

    //        System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
    //        panel.Location = new System.Drawing.Point(0, 0);
    //        panel.Dock = System.Windows.Forms.DockStyle.Fill;

    //        WindowsFormsHost host = new WindowsFormsHost();

    //        VitaminControl = new AxVitaminCtrl();
    //        VitaminControl.Dock = System.Windows.Forms.DockStyle.Fill;
    //        VitaminControl.Enabled = true;
    //        VitaminControl.Location = new System.Drawing.Point(0, 0);
    //        VitaminControl.BeginInit();
    //        VitaminControl.OnConnectionOK += VitaminControl_OnConnectionOK;
    //        VitaminControl.OnConnectionBroken += VitaminControl_OnConnectionBroken;
    //        //VitaminControl.OnVideoCodec += VitaminControl_OnVideoCodec;
    //        //VitaminControl.OnNewVideo += VitaminControl_OnNewVideo;
    //        VitaminControl.OnNewPacket += VitaminControl_OnNewPacket;

    //        panel.Controls.Add(VitaminControl);
    //        VitaminControl.EndInit();

    //        host.Child = panel;

    //        Container.Content = host;
    //        VitaminControl.UserName = "root";
    //        VitaminControl.Password = "hd543211";
    //        VitaminControl.ControlType = EControlType.eCtrlNormal;
    //        VitaminControl.IgnoreCaption = true;

    //        VitaminControl.IgnoreBorder = true;
    //        VitaminControl.AutoReconnect = true;
    //        VitaminControl.MediaType = EMediaType.eMediaVideo;
    //        //VitaminControl.RemoteIPAddr = IPAddress;
    //        VitaminControl.ServerModelType = EServerModelType.esrv7KServer;

    //        // 17-08-2016
    //        VitaminControl.ConnectionProtocol = VITAMINDECODERLib.EConnProtocol.eProtTCP;
    //        VitaminControl.AutoStartConnection = true;
    //        //____________________________________________________________________________________
    //        // 17-08-2016
    //        VitaminControl.ConnectionProtocol = EConnProtocol.eProtTCP;

    //        //VitaminControl.ConnectionProtocol = EConnProtocol.eProtUDP;
    //        VitaminControl.ViewStream = EDualStreamOption.eStream1;
    //        VitaminControl.NotifyImageFormat = EPictureFormat.ePicFmtYV12;
    //        VitaminControl.VideoQuality2K = EVideoQuality2K.evqua2KGood;
    //        VitaminControl.VideoSize2K = EVideoSize2K.evsz2KNormal;
    //        VitaminControl.DisplayErrorMsg = false;
    //        //VitaminControl.NotifyVideoData = true;
    //        VitaminControl.NotifyVideoPacket = true;
    //        // Create overlay
    //        Overlay = new CameraOverlay()
    //        {
    //            Container = this.Container,
    //            TextContent = VitaminControl.RemoteIPAddr,
    //        };

    //        IPAddress = ip;
    //    }
    //    //void VitaminControl_Click(object sender, EventArgs e)
    //    //{
    //    //    MessageBox.Show("click");
    //    //}

    //    void SetOverlay(string content)
    //    {
    //        if (this.Overlay != null)
    //        {
    //            Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
    //            {
    //                this.Overlay.TextContent = content;
    //            });
    //        }
    //    }

    //    void VitaminControl_OnNewPacket(object sender, _IVitaminCtrlEvents_OnNewPacketEvent e)
    //    {
    //        if (!_connectionOK)
    //            return;

    //        LoadZoomState(this.ZoomFactor);
    //    }

    //    public void Load(string ip, bool zoomable, ZoomFactor zoomFactor)
    //    {
    //        if (VitaminControl == null || VitaminControl.RemoteIPAddr==null)
    //        {
    //            Setup(ip);
    //            this.ZoomFactor = zoomFactor;    
    //        }
    //        else if (!VitaminControl.RemoteIPAddr.Equals(ip))
    //        {
    //            VitaminControl.Disconnect();
    //            IPAddress = ip;
    //            this.ZoomFactor = zoomFactor;
    //            VitaminControl.Connect();
    //        }

    //        if (zoomable)
    //        {
    //            //this.ZoomFactor = zoomFactor;
    //            VitaminControl.ControlButtonOpts =
    //                (int)(EControlButtonState.ebutDigitalZoom | EControlButtonState.ebutRtspPlayStop);
    //        }
    //        else
    //        {
    //            VitaminControl.ControlButtonOpts = (int)EControlButtonState.ebutRtspPlayStop;
    //        }
    //    }

    //    private void ReloadCamera(string ip)
    //    {
    //        IPAddress = ip;
    //        VitaminControl.NotifyVideoPacket = true;
    //        VitaminControl.ConnectionProtocol = VITAMINDECODERLib.EConnProtocol.eProtTCP;
    //    }

    //    public void ChangeIPAddress(string ip)
    //    {
    //        if (VitaminControl == null)
    //            Setup(ip);
    //        else if (!VitaminControl.RemoteIPAddr.Equals(ip))
    //        {
    //            VitaminControl.Disconnect();
    //            ReloadCamera(ip);
    //            VitaminControl.Connect();
    //        }
    //    }

    //    void VitaminControl_OnConnectionBroken(object sender, _IVitaminCtrlEvents_OnConnectionBrokenEvent e)
    //    {
    //        //Console.WriteLine("Connection broken");
    //        _connectionOK = false;
    //        _timeSinceConnectionOK = 0;
    //    }

    //    void VitaminControl_OnConnectionOK(object sender, _IVitaminCtrlEvents_OnConnectionOKEvent e)
    //    {
    //        //Console.WriteLine("Connection OK");
    //        _connectionOK = true;
    //        _timeSinceConnectionOK = System.Environment.TickCount;
    //    }

    //    public int DeviceId { get; set; }

    //    public event FrameEventHandler OnFrameReceived;

    //    public event ZoomEventHandler OnZoomReceived;

    //    public void FrameReceived(FrameEventArgs arg)
    //    {
    //        FrameEventHandler handler = OnFrameReceived;

    //        if (handler != null)
    //            handler(this, arg);
    //    }

    //    public void ZoomReceived(ZoomEventArgs arg)
    //    {
    //        ZoomEventHandler handler = OnZoomReceived;

    //        if (handler != null && _isZoomFactorChanged)
    //        {
    //            _isZoomFactorChanged = false;
    //            handler(this, arg);
    //        }
    //    }

    //    /// <summary>
    //    /// Starts capturing and sending image data.
    //    /// </summary>
    //    public void Start()
    //    {
    //        LoadZoomState(this.ZoomFactor);
    //        VitaminControl.Connect();
    //    }

    //    /// <summary>
    //    /// Capture current frame image
    //    /// </summary>
    //    /// <returns></returns>
    //    public byte[] CaptureImage(string watermark = "")
    //    {
    //        try
    //        {
    //            byte[] bytes = CaptureZoom(watermark);
    //            SaveZoomState();
    //            return bytes;

    //            //if (_canUse && _timeSinceConnectionOK > 0 && System.Environment.TickCount - _timeSinceConnectionOK > 2000)
    //            //{
    //            //    object data, info;
    //            //    byte[] bData;
    //            //    Array arrInfo;

    //            //    int a = VitaminControl.GetSnapshot(EPictureFormat.ePicFmtJpeg, out data, out info);
    //            //    bData = (byte[])data;
    //            //    arrInfo = (Array)info;
    //            //    SaveZoomState();
    //            //    return bData;
    //            //}
    //            //else
    //            //{
    //            //    Mvx.Resolve<ILogService>().Log(new Exception("Cannot capture image"));
    //            //    return null;
    //            //}
    //        }
    //        catch (Exception ex)
    //        {
    //            Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
    //            return null;
    //        }
    //    }

    //    public System.Drawing.Image CaptureImage()
    //    {
    //        var img = GetCameraBitMap();
    //        return (System.Drawing.Image)img;
    //    }

    //    public Bitmap GetCameraBitMap()
    //    {
    //        Bitmap bmp = null;
    //        lock (this)
    //        {
    //            try
    //            {
    //                IntPtr pt = new IntPtr(VitaminControl.DrawHwnd);

    //                //Bitmap bmp = null;
    //                IntPtr bmpDC;
    //                //Graphics g;
    //                IntPtr cameraDC;
    //                RECT windowRect = new RECT(0, 0, 0, 0);
    //                GetWindowRect(pt, out windowRect);

    //                cameraDC = GetWindowDC(pt);

    //                if (cameraDC != IntPtr.Zero)
    //                {
    //                    bmp = new Bitmap(windowRect.Width, windowRect.Height);

    //                    using (Graphics g = System.Drawing.Graphics.FromImage(bmp))
    //                    {
    //                        bmpDC = g.GetHdc();
    //                        BitBlt(bmpDC, 0, 0, windowRect.Width, windowRect.Height, cameraDC, 0, 0, TernaryRasterOperations.SRCCOPY);
    //                        g.ReleaseHdc(bmpDC);
    //                    }
    //                    ReleaseDC(pt, cameraDC);
    //                    return bmp;
    //                }

    //                return null;
    //            }
    //            catch (Exception ex)
    //            {
    //                Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
    //                return null;
    //            }
    //            finally
    //            {
    //                if (bmp != null)
    //                {
    //                    //bmp.Dispose();
    //                    //bmp = null;
    //                }
    //            }
    //        }
    //    }

    //    public byte[] CaptureZoom(string waterMark = "")
    //    {
    //        Bitmap bitMap = null;
    //        lock (this)
    //        {
    //            try
    //            {
    //                bitMap = GetCameraBitMap();
    //                if (bitMap == null) return null;

    //                ImageUtility.Watermark(bitMap, waterMark);
    //                return bitMap.ToByteArray(ImageFormat.Jpeg);
    //            }
    //            catch (Exception ex)
    //            {
    //                Mvx.Resolve<ILogService>().Log(new Exception("Capture image exception: " + ex.ToString()));
    //                return null;
    //            }
    //            finally
    //            {
    //                if (bitMap != null)
    //                {
    //                    bitMap.Dispose();
    //                    bitMap = null;
    //                }
    //            }
    //        }
    //    }

    //    public void SaveZoomState()
    //    {
    //        try
    //        {
    //            if (this.ZoomFactor == null)
    //                this.ZoomFactor = new ZoomFactor();
    //            if (ZoomFactor.Factor != VitaminControl.DigitalZoomFactor ||
    //                ZoomFactor.ZoomX != VitaminControl.DigitalZoomX ||
    //                ZoomFactor.ZoomY != VitaminControl.DigitalZoomY ||
    //                ZoomFactor.ZoomEnabled != VitaminControl.DigitalZoomEnabled)
    //            {
    //                this.ZoomFactor.ZoomEnabled = VitaminControl.DigitalZoomEnabled;
    //                this.ZoomFactor.Factor = VitaminControl.DigitalZoomFactor;   
    //                this.ZoomFactor.ZoomX = VitaminControl.DigitalZoomX;
    //                this.ZoomFactor.ZoomY = VitaminControl.DigitalZoomY;
    //                //SetOverlay(_ipAddress + " - " + this.ZoomFactor.Factor + "%");
    //                IsZoomFactorChanged = true;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Mvx.Resolve<ILogService>().Log(new Exception("Save zoom exception: " + ex.ToString()));
    //        }
    //    }

    //    private bool CheckZoomStateCorrect(ZoomFactor factor)
    //    {

    //        float a = Math.Abs((float)VitaminControl.DigitalZoomFactor - factor.Factor) / (float)factor.Factor;
    //        float b = Math.Abs((float)VitaminControl.DigitalZoomX - factor.ZoomX);
    //        float c = Math.Abs((float)VitaminControl.DigitalZoomY - factor.ZoomY);

    //        //SetOverlay(_ipAddress + " - " + this.ZoomFactor.Factor + "%");

    //        if ((a <= 0.1 && b <= 80 && c <= 80 && factor.ZoomEnabled == VitaminControl.DigitalZoomEnabled) ||
    //            (factor.ZoomEnabled == false && VitaminControl.DigitalZoomEnabled == false))
    //        {
    //            return true;
    //        }
    //        else
    //            return false;
    //    }

    //    public void LoadZoomState(ZoomFactor factor)
    //    {
    //        try
    //        {
    //            if (factor != null)
    //            {
    //                this.ZoomFactor = factor;

    //                VitaminControl.DigitalZoomEnabled = this.ZoomFactor.ZoomEnabled;
    //                VitaminControl.DigitalZoomFactor = (int)this.ZoomFactor.Factor;

    //                VitaminControl.DigitalZoomX = (int)this.ZoomFactor.ZoomX;
    //                VitaminControl.DigitalZoomY = (int)this.ZoomFactor.ZoomY;
    //                //VitaminControl.NotifyVideoPacket = false;
    //                //_canUse = true;
    //                if (CheckZoomStateCorrect(factor))
    //                {
    //                    VitaminControl.NotifyVideoPacket = false;
    //                    _canUse = true;
    //                }
    //            }
    //            else
    //                _canUse = true;
    //        }
    //        catch (Exception ex)
    //        {
    //            Mvx.Resolve<ILogService>().Log(new Exception("Load zoom exception: " + ex.ToString()));
    //        }
    //    }

    //    public void Pause()
    //    {
    //        VitaminControl.RtspPause();
    //    }

    //    public void Continue()
    //    {
    //        VitaminControl.RtspPlay();
    //    }

    //    public void Stop()
    //    {
    //        SaveZoomState();
    //        if (VitaminControl != null &&
    //            (VitaminControl.ControlStatus == EControlStatus.ctrlRunning ||
    //            VitaminControl.ControlStatus == EControlStatus.ctrlConnecting ||
    //            VitaminControl.ControlStatus == EControlStatus.ctrlReConnecting))
    //        {
    //            VitaminControl.CloseConnect();
    //        }

    //        _timeSinceConnectionOK = 0;
    //    }

    //    public void Dispose()
    //    {

    //    }

    //    [DllImport("user32.dll", SetLastError = true)]
    //    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    //    [DllImport("user32.dll")]
    //    static extern IntPtr GetWindowDC(IntPtr hWnd);
    //    [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
    //    [DllImport("user32.dll")]
    //    static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
    //}
    /// <summary>
    /// New 2018aug22
    /// </summary>
    #endregion
    public class VitaminCamera : ICamera
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
        //public UserControl Container { get { return VitaminControl.Container; } }

        public AxVitaminCtrl VitaminControl { get; set; }

        public CameraOverlay Overlay { get; private set; }

        bool _canUse = false;
        bool _connectionOK = false;
        int _timeSinceConnectionOK = 0;
        private int _handle;
        private IntPtr _playWndHandle = IntPtr.Zero;
        string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (_ipAddress == value)
                    return;
                _ipAddress = value;
                if (VitaminControl != null)
                {
                    VitaminControl.RemoteIPAddr = _ipAddress;
                    //ChangeIPAddress(_ipAddress, _port, _userName, _password);
                }
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
                if (VitaminControl != null)
                {
                    VitaminControl.UserName = _userName;
                    //ChangeIPAddress(_ipAddress, _port, _userName, _password);
                }
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
                if (VitaminControl != null)
                {
                    VitaminControl.Password = _password;
                    //ChangeIPAddress(_ipAddress, _port, _userName, _password);
                }
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
                //if (VitaminControl != null)
                    ;// VitaminControl.HttpPort = (int)_port;
            }
        }
        string _way_type;
        public string WayType
        {
            get { return _way_type; }
            set
            {
                if (_way_type == value)
                    return;
                _way_type = value;                
            }
        }
        //byte[] _data = null;
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
        public VitaminCamera()
        {

        }

        public VitaminCamera(string ip)
        {

        }
        /// <summary>
        /// Add window handle to display device's image
        /// </summary>
        /// <param name="wndHandle"></param>
        public void AddWndHandle(IntPtr wndHandle)
        {
            _playWndHandle = wndHandle;
        }
        public PictureBox PicBox { get; set; }
        public System.Windows.Forms.Panel Pnl { get; set; }
        System.Windows.Forms.Button btn;
        private System.Windows.Forms.Label lbl;
        private System.Windows.Forms.ToolTip toolTip1;
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
        public void Setup(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            Container = new System.Windows.Controls.UserControl();

            System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
            
            panel.Location = new System.Drawing.Point(0, 0);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;

            PicBox = new PictureBox();
            PicBox.SizeMode = PictureBoxSizeMode.Zoom;
            PicBox.Location = new System.Drawing.Point(0, 0);
            //PicBox.Dock = DockStyle.Fill;
            WindowsFormsHost host = new WindowsFormsHost();
            VitaminControl = new AxVitaminCtrl();
            //VitaminControl.Dock = System.Windows.Forms.DockStyle.Fill;
            VitaminControl.Enabled = true;
            VitaminControl.Location = new System.Drawing.Point(0, 0);
            VitaminControl.BeginInit();
            VitaminControl.OnConnectionOK += VitaminControl_OnConnectionOK;
            VitaminControl.OnConnectionBroken += VitaminControl_OnConnectionBroken;
            //VitaminControl.OnVideoCodec += VitaminControl_OnVideoCodec;
            //VitaminControl.OnNewVideo += VitaminControl_OnNewVideo;
            VitaminControl.OnNewPacket += VitaminControl_OnNewPacket;
            VitaminControl.Width = PicBox.Width;
            PicBox.Controls.Add(VitaminControl);
            
            Pnl = new System.Windows.Forms.Panel();
            Pnl.Visible = false;

            System.Windows.Forms.Button btnZoomin = new System.Windows.Forms.Button();
            
            btnZoomin.FlatStyle = FlatStyle.Flat;
            btnZoomin.FlatAppearance.BorderSize = 0;

            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnZoomin.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnZoomin.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

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
            
            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnZoomout.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnZoomout.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

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
            
            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnLeft.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnLeft.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

            btnLeft.ForeColor = System.Drawing.Color.White;
            
            btnLeft.Width = 25;
            btnLeft.Height = 25;
            btnLeft.Left = btnZoomout.Left + btnZoomout.Width + 25;

            btnLeft.Text = "<";
            btnLeft.Click += BtnLeft_Click;
            
            System.Windows.Forms.Button btnUp = new System.Windows.Forms.Button();
            btnUp.FlatStyle = FlatStyle.Flat;
            btnUp.FlatAppearance.BorderSize = 0;

            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnUp.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnUp.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

            btnUp.ForeColor = System.Drawing.Color.White;

            btnUp.Width = 25;
            btnUp.Height = 25;
            btnUp.Left = btnLeft.Left + btnZoomout.Width + 5;
            btnUp.Text = "^";
            
            btnUp.Click += BtnUp_Click;
            System.Windows.Forms.Button btnDown = new System.Windows.Forms.Button();
            btnDown.FlatStyle = FlatStyle.Flat;
            btnDown.FlatAppearance.BorderSize = 0;
            
            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnDown.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnDown.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

            btnDown.ForeColor = System.Drawing.Color.White;

            btnDown.Width = 25;
            btnDown.Height = 25;
            btnDown.Left = btnUp.Left + btnZoomout.Width + 5;
            btnDown.Text = "v";
            btnDown.Click += BtnDown_Click;   
           
            System.Windows.Forms.Button btnRight = new System.Windows.Forms.Button();
            btnRight.FlatStyle = FlatStyle.Flat;
            btnRight.FlatAppearance.BorderSize = 0;
            
            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btnRight.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btnRight.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

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

            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    btn.BackColor = System.Drawing.Color.FromArgb(55, 189, 176);
                else
                    btn.BackColor = System.Drawing.Color.FromArgb(49, 142, 204);

            btn.ForeColor = System.Drawing.Color.White;
            btn.BackgroundImage = Properties.Resources.config_camera;
            btn.BackgroundImageLayout = ImageLayout.Zoom;
            
            btn.Dock = DockStyle.Right;
            btn.Click += Btn_Click;
            System.Windows.Forms.Panel subpnl = new System.Windows.Forms.Panel();
            
            subpnl.Dock = DockStyle.Bottom;
            subpnl.Height = 30;
            if (waytype != null)
                if (waytype.ToUpper() == "OUT")
                    subpnl.BackColor = System.Drawing.Color.FromArgb(125, 55, 189, 176);
                else
                    subpnl.BackColor = System.Drawing.Color.FromArgb(125, 49, 142, 204);
            lbl.Left = 5;

            Pnl.Dock = DockStyle.Bottom;
            Pnl.Height = subpnl.Height;

            lbl.Top = (subpnl.Height-lbl.Height)/2+3;

            btnZoomin.Top = btnZoomout.Top = btnLeft.Top = btnRight.Top = btnUp.Top = btnDown.Top = (subpnl.Height - btnZoomin.Height) / 2;
            
            subpnl.Controls.Add(lbl);
            subpnl.Controls.Add(btn);
            subpnl.Controls.Add(Pnl);
            subpnl.Controls.SetChildIndex(Pnl, 999);
            subpnl.Controls.SetChildIndex(lbl, -1);
            subpnl.Controls.SetChildIndex(btn, 9);

            System.Windows.Forms.ContextMenu cm = new System.Windows.Forms.ContextMenu();
            //lbl.DoubleClick += Lbl_DoubleClick;
            //lbl.MouseDown += Lbl_MouseDown;
            
            panel.Controls.Add(subpnl);
            panel.Controls.Add(PicBox);
            VitaminControl.EndInit();
            panel.Controls.SetChildIndex(PicBox, -1);
            panel.Controls.SetChildIndex(subpnl, 9);
            panel.SizeChanged += Panel_SizeChanged;
            host.Child = panel;
           
            Container.Content = host;
            VitaminControl.UserName = username;
            VitaminControl.Password = password;
            VitaminControl.ControlType = EControlType.eCtrlNoCtrlBar;
            VitaminControl.IgnoreCaption = true;

            VitaminControl.IgnoreBorder = true;
            VitaminControl.AutoReconnect = true;
            VitaminControl.MediaType = EMediaType.eMediaVideo;
            VitaminControl.RemoteIPAddr = ip;
            VitaminControl.ServerModelType = EServerModelType.esrv7KServer; 
            // 17-08-2016
            VitaminControl.ConnectionProtocol = VITAMINDECODERLib.EConnProtocol.eProtTCP;
            VitaminControl.AutoStartConnection = true;
            //____________________________________________________________________________________
            // 17-08-2016
            VitaminControl.ConnectionProtocol = EConnProtocol.eProtTCP;
            
            //VitaminControl.ConnectionProtocol = EConnProtocol.eProtUDP;
            VitaminControl.ViewStream = EDualStreamOption.eStream1;
            VitaminControl.NotifyImageFormat = EPictureFormat.ePicFmtYV12;
            VitaminControl.VideoQuality2K = EVideoQuality2K.evqua2KGood;
            VitaminControl.VideoSize2K = EVideoSize2K.evsz2KNormal;
            VitaminControl.DisplayErrorMsg = false;
            //VitaminControl.NotifyVideoData = true;
            VitaminControl.NotifyVideoPacket = true;
            // Create overlay
            //Overlay = new CameraOverlay()
            //{
            //    Container = this.Container,
            //    TextContent = VitaminControl.RemoteIPAddr,
            //};
            PicBox.Width = panel.Width;
            PicBox.Height = panel.Height - lbl.Height;
            _ipAddress = ip;
            _port = port;
            _userName = username;
            _password = password;
            _way_type = waytype;
            AddWndHandle(PicBox.Handle);
        }

        private void Panel_SizeChanged(object sender, EventArgs e)
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
                float Zoomx = this.ZoomFactor.ZoomX +factor/2;
                float Zoomy = this.ZoomFactor.ZoomY + factor / 2;
                if (Factor<=400)
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
            if(this.ZoomFactor!=null)
            {     
                float Factor = this.ZoomFactor.Factor;
                float ScaleW = (float)PicBox.Width / (float)this.ZoomFactor.BoxWidth;
                float ScaleH = (float)PicBox.Height / (float)this.ZoomFactor.BoxHeight;
                float Zoomx= this.ZoomFactor.ZoomX * ScaleW;
                float Zoomy = this.ZoomFactor.ZoomY * ScaleH;
                if (Factor > 100 && Factor <= 400)
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
                    int tw = (int)(pw * (Factor / 100));
                    int th = (int)(ph * (Factor / 100));
                    float maxleft = tw - pw;
                    float maxtop = th - ph;
                    if(Zoomx > maxleft)
                    {
                        Zoomx = maxleft;      
                    }
                    if (Zoomy >maxtop)
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
                    VitaminControl.Left = (int)(-Zoomx);
                    VitaminControl.Top = (int)(-Zoomy);
                    VitaminControl.Width = tw;
                    VitaminControl.Height = th;
                }
                else if(Factor>400)
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
                    int tw = 4 * pw;
                    int th = 4 * ph;
                    VitaminControl.Width = 4*pw;
                    VitaminControl.Height = 4*ph;
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
                    VitaminControl.Left = (int)(-Zoomx);
                    VitaminControl.Top = (int)(-Zoomy);
                    VitaminControl.Width = tw;
                    VitaminControl.Height = th;
                }
                else
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
                    this.ZoomFactor.ZoomX = 0;
                    this.ZoomFactor.ZoomY = 0;
                    this.ZoomFactor.BoxWidth = pw;
                    this.ZoomFactor.BoxHeight = ph;
                    VitaminControl.Left = 0;
                    VitaminControl.Top = 0;
                    VitaminControl.Width = pw;
                    VitaminControl.Height = ph;
                }
                toolTip1.ToolTipTitle = "{ Factor: " + this.ZoomFactor.Factor + "%, Left: "+ ((int)this.ZoomFactor.ZoomX).ToString()+ 
                    ", Top:"+ ((int)this.ZoomFactor.ZoomY).ToString() + ", Width:" + ((int)this.ZoomFactor.BoxWidth).ToString() + ", Height:" + ((int)this.ZoomFactor.BoxHeight).ToString() + "}";
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

        void VitaminControl_OnNewPacket(object sender, _IVitaminCtrlEvents_OnNewPacketEvent e)
        {
            if (!_connectionOK)
                return;

            LoadZoomState(this.ZoomFactor);
        }
		public TrackerFactor TrackerFactor { get; set; }
		public TrackerEvent TrackerEvent { get; set; }

		public bool BlueTriggerStatus => false;

		public bool RedTriggerStatus => false;
		public void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel = 1)
        {
            
            if (VitaminControl == null || VitaminControl.RemoteIPAddr == null)
            {
                Setup(ip, port, username, password, waytype);
                if (zoomFactor != null)
                    this.ZoomFactor = zoomFactor;
                else
                    this.ZoomFactor = new ZoomFactor();
                VitaminControl.Connect();
            }
            else if (!VitaminControl.RemoteIPAddr.Equals(ip) || VitaminControl.UserName.Equals(username)|| VitaminControl.Password.Equals(password))
            {
                VitaminControl.Disconnect();
                IPAddress = ip;
                Password = password;
                UserName = username;
                VitaminControl.UserName = username;
                VitaminControl.Password = password; 
                if (zoomFactor != null)
                    this.ZoomFactor = zoomFactor;
                else
                    this.ZoomFactor = new ZoomFactor();
                VitaminControl.Connect();
                
            }

            if (zoomable)
            {
                //this.ZoomFactor = zoomFactor;
                VitaminControl.ControlButtonOpts =
                    (int)(EControlButtonState.ebutDigitalZoom | EControlButtonState.ebutRtspPlayStop);
            }
            else
            {
                VitaminControl.ControlButtonOpts = (int)EControlButtonState.ebutMicVolume;
            }
        }

        private void ReloadCamera(string ip, string port, string username, string password)
        {
            IPAddress = ip;
            Port = port;
            UserName = username;
            Password = password;
            VitaminControl.UserName = username;
            VitaminControl.Password = password;
            VitaminControl.NotifyVideoPacket = true;
            VitaminControl.ConnectionProtocol = VITAMINDECODERLib.EConnProtocol.eProtTCP;
        }

        public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            if (VitaminControl == null)
                Setup(ip, port, username, password,waytype);
            else if (!VitaminControl.RemoteIPAddr.Equals(ip) || VitaminControl.UserName.Equals(username) || VitaminControl.Password.Equals(password))
            {
                VitaminControl.Disconnect();
                ReloadCamera(ip, port, username, password);
                VitaminControl.Connect();
            }
        }

        void VitaminControl_OnConnectionBroken(object sender, _IVitaminCtrlEvents_OnConnectionBrokenEvent e)
        {
            //Console.WriteLine("Connection broken");
            _connectionOK = false;
            _timeSinceConnectionOK = 0;
        }
        private void TryResize()
        {
            try
            {
                System.Windows.Forms.Panel pnl = PicBox.Parent as System.Windows.Forms.Panel;
                foreach (var control in pnl.Controls)
                {
                    if (control is System.Windows.Forms.Panel)
                    {
                        System.Windows.Forms.Panel lbl = control as System.Windows.Forms.Panel;
                        if (PicBox.Width != pnl.Width)
                            PicBox.Width = pnl.Width;
                        if (PicBox.Height != pnl.Height - lbl.Height)
                            PicBox.Height = pnl.Height - lbl.Height;
                        if (VitaminControl.Width < PicBox.Width)
                            VitaminControl.Width = PicBox.Width;
                        if (VitaminControl.Height < PicBox.Height)
                            VitaminControl.Height = PicBox.Height;
                    }
                }
            }
            catch
            {
                ;
            }
        }
        void VitaminControl_OnConnectionOK(object sender, _IVitaminCtrlEvents_OnConnectionOKEvent e)
        {
            //Console.WriteLine("Connection OK");
            _connectionOK = true;
            _timeSinceConnectionOK = System.Environment.TickCount;
            TryResize();
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
            VitaminControl.Connect();
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
                SaveZoomState();
                return bytes;

                //if (_canUse && _timeSinceConnectionOK > 0 && System.Environment.TickCount - _timeSinceConnectionOK > 2000)
                //{
                //    object data, info;
                //    byte[] bData;
                //    Array arrInfo;

                //    int a = VitaminControl.GetSnapshot(EPictureFormat.ePicFmtJpeg, out data, out info);
                //    bData = (byte[])data;
                //    arrInfo = (Array)info;
                //    SaveZoomState();
                //    return bData;
                //}
                //else
                //{
                //    Mvx.Resolve<ILogService>().Log(new Exception("Cannot capture image"));
                //    return null;
                //}
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

                    Mvx.Resolve<IMvxMainThreadDispatcher>().RequestMainThreadAction(() =>
                    {
                        pt = PicBox.Handle;
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
                //if (ZoomFactor.Factor != VitaminControl.DigitalZoomFactor ||
                //    ZoomFactor.ZoomX != VitaminControl.DigitalZoomX ||
                //    ZoomFactor.ZoomY != VitaminControl.DigitalZoomY ||
                //    ZoomFactor.ZoomEnabled != VitaminControl.DigitalZoomEnabled)
                //{
                //    this.ZoomFactor.ZoomEnabled = VitaminControl.DigitalZoomEnabled;
                //    this.ZoomFactor.Factor = VitaminControl.DigitalZoomFactor;
                //    this.ZoomFactor.ZoomX = VitaminControl.DigitalZoomX;
                //    this.ZoomFactor.ZoomY = VitaminControl.DigitalZoomY;
                //    //SetOverlay(_ipAddress + " - " + this.ZoomFactor.Factor + "%");
                //    IsZoomFactorChanged = true;
                //}
            }
            catch (Exception ex)
            {
                Mvx.Resolve<ILogService>().Log(new Exception("Save zoom exception: " + ex.ToString()));
            }
        }

        private bool CheckZoomStateCorrect(ZoomFactor factor)
        {

            float a = Math.Abs((float)VitaminControl.DigitalZoomFactor - factor.Factor) / (float)factor.Factor;
            float b = Math.Abs((float)VitaminControl.DigitalZoomX - factor.ZoomX);
            float c = Math.Abs((float)VitaminControl.DigitalZoomY - factor.ZoomY);

            //SetOverlay(_ipAddress + " - " + this.ZoomFactor.Factor + "%");

            if ((a <= 0.1 && b <= 80 && c <= 80 && factor.ZoomEnabled == VitaminControl.DigitalZoomEnabled) ||
                (factor.ZoomEnabled == false && VitaminControl.DigitalZoomEnabled == false))
            {
                return true;
            }
            else
                return false;
        }

        public void LoadZoomState(ZoomFactor factor)
        {
            try
            {
                //if (factor != null)
                //{
                //    this.ZoomFactor = factor;

                //    VitaminControl.DigitalZoomEnabled = this.ZoomFactor.ZoomEnabled;
                //    VitaminControl.DigitalZoomFactor = (int)this.ZoomFactor.Factor;

                //    VitaminControl.DigitalZoomX = (int)this.ZoomFactor.ZoomX;
                //    VitaminControl.DigitalZoomY = (int)this.ZoomFactor.ZoomY;
                //    //VitaminControl.NotifyVideoPacket = false;
                //    //_canUse = true;
                //    if (CheckZoomStateCorrect(factor))
                //    {
                //        VitaminControl.NotifyVideoPacket = false;
                //        _canUse = true;
                //    }
                //}
                //else
                //    _canUse = true;
                this.ZoomFactor = factor;
                //TryResize();
                //ReZoom();
            }
            catch (Exception ex)
            {
                Mvx.Resolve<ILogService>().Log(new Exception("Load zoom exception: " + ex.ToString()));
            }
        }

        public void Pause()
        {
            VitaminControl.RtspPause();
        }

        public void Continue()
        {
            VitaminControl.RtspPlay();
        }

        public void Stop()
        {
            SaveZoomState();
            if (VitaminControl != null &&
                (VitaminControl.ControlStatus == EControlStatus.ctrlRunning ||
                VitaminControl.ControlStatus == EControlStatus.ctrlConnecting ||
                VitaminControl.ControlStatus == EControlStatus.ctrlReConnecting))
            {
                VitaminControl.CloseConnect();
            }

            _timeSinceConnectionOK = 0;
        }

        public void Dispose()
        {

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
    }
}