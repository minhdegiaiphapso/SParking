using SP.Parking.Terminal.Wpf.RtspSupport.RawFramesReceiving;
using RtspClientSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SP.Parking.Terminal.Wpf.RtspSupport
{
    enum ZoomState
    {
        None,
        Out,
        In,
        Left,
        Right,
        Up,
        Down
    }

    /// <summary>
    /// Interaction logic for RtspPlayer.xaml
    /// </summary>
    public partial class RtspPlayer : UserControl
    {
        public RtspPlayer()
        {
            InitializeComponent();
            this.Loaded += RtspPlayer_Loaded;
            this.Unloaded += RtspPlayer_Unloaded;
            this.SizeChanged += RtspPlayer_SizeChanged;
            zoomTimer = new DispatcherTimer();
            zoomTimer.Interval = TimeSpan.FromMilliseconds(10);
            zoomTimer.Tick += ZoomTimer_Tick;
            ZoomAction = ZoomState.None;
            SetZoom(100, 0, 0);
        }
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            pnlConfig.Visibility = Visibility.Visible;
        }
        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            pnlConfig.Visibility = Visibility.Hidden;
        }
        #region Zoom
        private void RtspPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ActiveZoom();
        }
        public void SetZoom(int percent, double X, double Y)
        {
            ZoomPercent = percent;
            XPosition = X;
            YPosition = Y;
            OutStep();
            InStep();
            ActiveZoom();
        }
        private void ActiveZoom()
        {
            try
            {
                boder.Width = grdparent.ActualWidth;
                boder.Height = grdparent.ActualHeight;
                var w = (int)(grdparent.ActualWidth * (ZoomPercent / 100.0f));
                var h = (int)(grdparent.ActualHeight * (ZoomPercent / 100.0f));
                var l = (int)(w * XPosition);
                var t = (int)(h * YPosition);
                container.Width = w;
                container.Height = h;
                container.Margin = new Thickness(-l, -t, 0, 0);
            }
            catch
            {
                ;
            }
        }
        public double YPosition { get;  set; }

        public int ZoomPercent { get;  set; }

        public double XPosition { get;  set; }

        private ZoomState ZoomAction;

        private DispatcherTimer zoomTimer;
        private void ZoomTimer_Tick(object sender, EventArgs e)
        {
            switch (ZoomAction)
            {
                case ZoomState.None:
                    zoomTimer.Stop();
                    ZoomAction = ZoomState.None;
                    break;
                case ZoomState.Out:
                    OutStep();
                    break;
                case ZoomState.In:
                    InStep();
                    break;
                case ZoomState.Left:
                    LeftStep();
                    break;
                case ZoomState.Right:
                    RightStep();
                    break;
                case ZoomState.Up:
                    UpStep();
                    break;
                case ZoomState.Down:
                    DownStep();
                    break;
            }
            ActiveZoom();
           
        }
        private void DownStep()
        {

            var tmp = YPosition * 100.0 - 1.0;
            if (tmp <= 0)
            {
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
                YPosition = 0;
            }
            else
            {
                YPosition = tmp / 100.0;
            }
        }
        private void UpStep()
        {
            var maxPercent = (1.0 - 100 * 1.0 / ZoomPercent * 1.0);
            var tmp = YPosition * 100.0 + 1.0;
            YPosition = tmp / 100.0;
            if (YPosition >= maxPercent)
            {
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
                YPosition = maxPercent;
            }
        }

        private void RightStep()
        {
            var tmp = XPosition * 100.0 - 1.0;
            if (tmp <= 0)
            {
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
                XPosition = 0;
            }
            else
            {
                XPosition = tmp / 100.0;
            }
        }

        private void LeftStep()
        {
            var maxPercent = (1.0 - 100 * 1.0 / ZoomPercent * 1.0);
            var tmp = XPosition * 100.0 + 1.0;
            XPosition = tmp / 100.0;
            if (XPosition >= maxPercent)
            {
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
                XPosition = maxPercent;
            }
        }

        private void InStep()
        {
            ZoomPercent--;
            if (ZoomPercent <= 100)
            {
                ZoomPercent = 100;
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
            }
            var maxPercent = (1.0 - 100 * 1.0 / ZoomPercent * 1.0);
            if (XPosition > maxPercent)
                XPosition = maxPercent;
            if (YPosition > maxPercent)
                YPosition = maxPercent;
        }

        private void OutStep()
        {
            ZoomPercent++;
            if (ZoomPercent >= 400)
            {
                ZoomPercent = 400;
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
            }
            var maxPercent = (1.0 - 100 * 1.0 / ZoomPercent * 1.0);
            if (XPosition > maxPercent)
                XPosition = maxPercent;
            if (YPosition > maxPercent)
                YPosition = maxPercent;
        }

        private void BtnZoom_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ZoomState zoomSate = ZoomState.None;
            switch (btn.Name)
            {
                case "btnZoomOut":
                    zoomSate = ZoomState.Out;
                    break;
                case "btnZoomIn":
                    zoomSate = ZoomState.In;
                    break;
                case "btnZoomLeft":
                    zoomSate = ZoomState.Left;
                    break;
                case "btnZoomRight":
                    zoomSate = ZoomState.Right;
                    break;
                case "btnZoomUp":
                    zoomSate = ZoomState.Up;
                    break;
                case "btnZoomDown":
                    zoomSate = ZoomState.Down;
                    break;
            }
            DoZoom(zoomSate);
        }

        private void DoZoom(ZoomState zoomSate)
        {
            if (ZoomAction == ZoomState.None)
            {
                ZoomAction = zoomSate;
                zoomTimer.Start();
            }
            else
            {
                zoomTimer.Stop();
                ZoomAction = ZoomState.None;
            }
        }
        public Bitmap Capture()
        {
            return video.GetFrame;
        }
        #endregion

        #region Rtsp data
        #region  block 1
        private readonly RealtimeVideoSource _realtimeVideoSource = new RealtimeVideoSource();
        private readonly RealtimeAudioSource _realtimeAudioSource = new RealtimeAudioSource();

        private IRawFramesSource _rawFramesSource;

        public event EventHandler<string> StatusChanged;

        public IVideoSource VideoSource => _realtimeVideoSource;

        public void Start(ConnectionParameters connectionParameters)
        {
            if (!IsStart)
            {
                if (_rawFramesSource != null)
                    return;
                _rawFramesSource = new RawFramesSource(connectionParameters);
                _rawFramesSource.ConnectionStatusChanged += ConnectionStatusChanged;

                _realtimeVideoSource.SetRawFramesSource(_rawFramesSource);
                _realtimeAudioSource.SetRawFramesSource(_rawFramesSource);

                _rawFramesSource.Start();
                IsStart = true;
            }
        }

        public void Stop()
        {
            if (IsStart)
            {
                if (_rawFramesSource == null)
                {
                    return;
                }
                _rawFramesSource.Stop();
                Thread.Sleep(5);
                IsStart = false;
                _realtimeVideoSource.SetRawFramesSource(null);
                _rawFramesSource = null;
            }
        }

        private void ConnectionStatusChanged(object sender, string s)
        {
            StatusChanged?.Invoke(this, s);
        }
        #endregion
        #region Bolok 2
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";
        private string _Login = "admin";
        private string _Password = "hd543211";
        #endregion
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            OnStartButtonClick();
        }
        private bool IsStart = false;
        public void CallPlay(string IpAddress, string UserName, string Password)
        {
            CallStop();
            if (!IsStart)
            { 
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        txtAddress.Text = IpAddress;
                        _Login = UserName;
                        _Password = Password;
                    });
                OnStartButtonClick();
            }
        }
        public void CallStop()
        {    
            if(IsStart)
                OnStopButtonClick();
        }

        internal void ShowConfig(bool active)
        {
            if (active)
                pnlAction.Visibility = Visibility.Visible;
            else
                pnlAction.Visibility = Visibility.Hidden;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            OnStopButtonClick();
        }
        private void RtspPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }
        private void RtspPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            video.VideoSource = VideoSource;
        }
        private void OnStartButtonClick()
        {
            if (!IsStart)
            {
                string address = txtAddress.Text;

                if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                    address = RtspPrefix + address;

                if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
                {
                    MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var credential = new NetworkCredential(_Login, _Password);

                var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) :
                    new ConnectionParameters(deviceUri, credential);

                connectionParameters.RtpTransport = RtpTransportProtocol.TCP;
                connectionParameters.CancelTimeout = TimeSpan.FromSeconds(15);
                connectionParameters.ReceiveTimeout = TimeSpan.FromSeconds(5);

                Start(connectionParameters);
                if (IsStart)
                {
                    StatusChanged += OnStatusChanged;
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = true;
                    txtAddress.IsEnabled = false;
                    video.IsReady = true;
                }
            }
        }
        private void OnStopButtonClick()
        {
            if (IsStart)
            {
                video.IsReady = false;
                Stop();
                if (!IsStart)
                {
                    StatusChanged -= OnStatusChanged;
                    btnStop.IsEnabled = false;
                    btnStart.IsEnabled = true;
                    lblStatus.Text = string.Empty;
                    txtAddress.IsEnabled = true;
                }   
            }
        }
        private void OnStatusChanged(object sender, string s)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => lblStatus.Text = s);
        }
        #endregion


    }
}
