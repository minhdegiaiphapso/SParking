using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SP.Parking.Terminal.Core;
using SP.Parking.Terminal.Core.Utility;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using Cirrious.CrossCore.Core;
using SP.Parking.Terminal.Core.Utilities;

namespace SP.Parking.Terminal.Wpf.Devices
{
    public class BoschCamera : ICamera
    {
        public int DeviceId { get ; set; }
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
        public System.Windows.Controls.UserControl Container { get; set; }

        public event FrameEventHandler OnFrameReceived;
        public event ZoomEventHandler OnZoomReceived;

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
                    return bitMap.ToByteArray(System.Drawing.Imaging.ImageFormat.Jpeg);
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
        public void ChangeIPAddress(string ip, string port, string username, string password, string waytype, int channel = 1)
        {
            //if (!IPAddress.Equals(ip))
            //{
            //    BoschControl.IPAddress = ip;
            //    BoschControl.DisConnect();
            //    BoschControl.Connect();
            //}
        }

        public void Continue()
        {
            //BoschControl.Connect();
        }

        public void Dispose()
        {
            BoschControl.DisConnect();
            BoschControl.Dispose();
        }

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
		public TrackerFactor TrackerFactor { get; set; }
		public TrackerEvent TrackerEvent { get; set; }

		public bool BlueTriggerStatus => false;

		public bool RedTriggerStatus => false;
		public void Load(string ip, string port, string username, string password, bool zoomable, ZoomFactor zoomFactor, TrackerFactor trackerFactor, string waytype, int channel = 1)
        {
            if (BoschControl == null || BoschControl.IPAddress != ip|| this._port!=port|| this._userName!=username||this._password!=password)
            {
                Setup(ip, port, username, password, waytype);
                BoschControl.Connect();
            }
            if (zoomFactor != null)
                this.ZoomFactor = zoomFactor;
            else
                this.ZoomFactor = new ZoomFactor();
        }

        public void Pause()
        {
            BoschControl.DisConnect();
        }

        public void SaveZoomState()
        {
            
        }
        #region Bosch
        public BoschCamera()
        {

        }

        public BoschCamera(string ip)
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
        string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (_ipAddress == value)
                    return;
                _ipAddress = value;
                ChangeIPAddress(_ipAddress, _port, _userName, _password, _waytype);
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
                ChangeIPAddress(_ipAddress, _port, _userName, _password, _waytype);
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
                ChangeIPAddress(_ipAddress, _port, _userName, _password, _waytype);
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
                ChangeIPAddress(_ipAddress, _port, _userName, _password, _waytype);
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
                ChangeIPAddress(_ipAddress, _port, _userName, _password, _waytype);
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

        private IntPtr _playWndHandle = IntPtr.Zero;
        //private BoschControl.CamCtrol BoschControl;
        //private Bosch.VideoSDK.CameoLib.Cameo BoschCameo;
        public BoschControl.CamCtrol BoschControl { get; set; }
        public PictureBox PicBox { get; set; }
        public System.Windows.Forms.Panel Pnl { get; set; }


        private System.Windows.Forms.Button btn;
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
                        if (BoschControl.Width < PicBox.Width)
                            BoschControl.Width = PicBox.Width;
                        if (BoschControl.Height < PicBox.Height)
                            BoschControl.Height = PicBox.Height;
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
                float ScaleW = (float)PicBox.Width / (float)this.ZoomFactor.BoxWidth;
                float ScaleH = (float)PicBox.Height / (float)this.ZoomFactor.BoxHeight;
                float Zoomx = this.ZoomFactor.ZoomX * ScaleW;
                float Zoomy = this.ZoomFactor.ZoomY * ScaleH;
                if (Factor > 100 && Factor <= 400)
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
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
                    BoschControl.Left = (int)(-Zoomx);
                    BoschControl.Top = (int)(-Zoomy);
                    BoschControl.Width = tw;
                    BoschControl.Height = th;
                }
                else if (Factor > 400)
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
                    int tw = 4 * pw;
                    int th = 4 * ph;
                    BoschControl.Width = 4 * pw;
                    BoschControl.Height = 4 * ph;
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
                    BoschControl.Left = (int)(-Zoomx);
                    BoschControl.Top = (int)(-Zoomy);
                    BoschControl.Width = tw;
                    BoschControl.Height = th;
                }
                else
                {
                    int pw = PicBox.Width;
                    int ph = PicBox.Height;
                    this.ZoomFactor.ZoomX = 0;
                    this.ZoomFactor.ZoomY = 0;
                    this.ZoomFactor.BoxWidth = pw;
                    this.ZoomFactor.BoxHeight = ph;
                    BoschControl.Left = 0;
                    BoschControl.Top = 0;
                    BoschControl.Width = pw;
                    BoschControl.Height = ph;
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
        public void Setup(string ip, string port,string username, string password, string waytype, int channel = 1)
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
            //BoschControl = new BoschControl.CamCtrol();
            if (BoschControl == null)
                BoschControl = new BoschControl.CamCtrol();
            this.IPAddress = ip;
            BoschControl.IPAddress = ip;
            BoschControl.ProgId = "GCA.VIP.DeviceProxy";
            BoschControl.Width = PicBox.Width;
            BoschControl.Height = PicBox.Height;
            BoschControl.BackColor = System.Drawing.Color.Black;
            PicBox.Controls.Add(BoschControl);

            Pnl = new System.Windows.Forms.Panel();
            Pnl.Visible = false;
            //Pnl.BackColor = System.Drawing.Color.Green;//System.Drawing.Color.Black;
            Pnl.Width = 300;
            //Pnl.Dock = DockStyle.Left;   
            System.Windows.Forms.Button btnZoomin = new System.Windows.Forms.Button();
            btnZoomin.Location = new System.Drawing.Point(0, 0);
            btnZoomin.BackColor = System.Drawing.Color.Aqua;
            btnZoomin.Font = new Font("Arial", 10, System.Drawing.FontStyle.Bold);
            btnZoomin.ForeColor = System.Drawing.Color.Black;
            btnZoomin.Text = "-";
            btnZoomin.Width = 30;
            btnZoomin.Click += BtnZoomin_Click;
            System.Windows.Forms.Button btnZoomout = new System.Windows.Forms.Button();
            btnZoomout.Location = new System.Drawing.Point(50, 0);
            btnZoomout.BackColor = System.Drawing.Color.Aqua;
            btnZoomout.Font = new Font("Arial", 10, System.Drawing.FontStyle.Bold);
            btnZoomout.ForeColor = System.Drawing.Color.Black;
            btnZoomout.Text = "+";
            btnZoomout.Width = 30;
            btnZoomout.Click += BtnZoomout_Click;
            System.Windows.Forms.Button btnLeft = new System.Windows.Forms.Button();
            btnLeft.Location = new System.Drawing.Point(100, 0);
            btnLeft.BackColor = System.Drawing.Color.Aqua;
            btnLeft.Font = new Font("Arial", 10, System.Drawing.FontStyle.Bold);
            btnLeft.ForeColor = System.Drawing.Color.Black;
            btnLeft.Text = "<";
            btnLeft.Width = 30;
            btnLeft.Click += BtnLeft_Click;
            System.Windows.Forms.Button btnUp = new System.Windows.Forms.Button();
            btnUp.Location = new System.Drawing.Point(150, 0);
            btnUp.BackColor = System.Drawing.Color.Aqua;
            btnUp.Text = "^";
            btnUp.Font = new Font("Arial", 12, System.Drawing.FontStyle.Bold);
            btnUp.ForeColor = System.Drawing.Color.Black;
            btnUp.Width = 30;
            btnUp.Click += BtnUp_Click;
            System.Windows.Forms.Button btnDown = new System.Windows.Forms.Button();
            btnDown.Location = new System.Drawing.Point(200, 0);
            btnDown.BackColor = System.Drawing.Color.Aqua;
            btnDown.Text = "v";
            btnDown.Font = new Font("Arial", 9, System.Drawing.FontStyle.Bold);
            btnDown.ForeColor = System.Drawing.Color.Black;
            btnDown.Width = 30;
            btnDown.Click += BtnDown_Click;
            System.Windows.Forms.Button btnRight = new System.Windows.Forms.Button();
            btnRight.Location = new System.Drawing.Point(250, 0);
            btnRight.BackColor = System.Drawing.Color.Aqua;
            btnRight.Font = new Font("Arial", 10, System.Drawing.FontStyle.Bold);
            btnRight.ForeColor = System.Drawing.Color.Black;
            btnRight.Text = ">";
            btnRight.Width = 30;
            btnRight.Click += BtnRight_Click;
            Pnl.Controls.Add(btnZoomout);
            Pnl.Controls.Add(btnZoomin);
            Pnl.Controls.Add(btnLeft);
            Pnl.Controls.Add(btnUp);
            Pnl.Controls.Add(btnDown);
            Pnl.Controls.Add(btnRight);
            lbl = new System.Windows.Forms.Label();
            lbl.Dock = DockStyle.Bottom;
            lbl.BackColor = System.Drawing.Color.Azure;
            lbl.Text = ip;
            toolTip1 = new System.Windows.Forms.ToolTip();
            toolTip1.SetToolTip(lbl, "Địa chỉ: " + ip);
            btn = new System.Windows.Forms.Button();
            btn.Width = 30;
            btn.Text = "|||";
            btn.Dock = DockStyle.Right;
            btn.Click += Btn_Click;
            System.Windows.Forms.Panel subpnl = new System.Windows.Forms.Panel();
            subpnl.Dock = DockStyle.Bottom;
            subpnl.Height = 20;
            subpnl.Controls.Add(lbl);
            subpnl.Controls.Add(btn);
            subpnl.Controls.Add(Pnl);
            subpnl.Controls.SetChildIndex(Pnl, 999);
            subpnl.Controls.SetChildIndex(lbl, -1);
            subpnl.Controls.SetChildIndex(btn, 9);
            System.Windows.Forms.ContextMenu cm = new System.Windows.Forms.ContextMenu();
            panel.Resize += Panel_Resize;
            panel.Controls.Add(PicBox);
            panel.Controls.Add(subpnl);
            panel.Controls.SetChildIndex(PicBox, -1);
            panel.Controls.SetChildIndex(subpnl, 9);
            host.Child = panel;
            Container.LayoutUpdated += Container_LayoutUpdated;
            Container.Content = host;
            PicBox.Width = panel.Width;
            PicBox.Height = panel.Height - lbl.Height;
            _ipAddress = ip;
            _port = port;
            _userName = username;
            _password = password;
            AddWndHandle(PicBox.Handle);
        }

        private void Container_LayoutUpdated(object sender, EventArgs e)
        {
            //TryResize();
            //ReZoom();
        }

        public void Start()
        {    
            BoschControl.Connect();
            TryResize();
            ReZoom();
        }
        public void Stop()
        {
            BoschControl.DisConnect();
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
