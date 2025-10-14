using AxVITAMINDECODERLib;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.ViewModels;
using SP.Parking.Terminal.Wpf.Devices;
using SP.Parking.Terminal.Wpf.UI;
using Green.Devices.Dal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VITAMINDECODERLib;
using SP.Parking.Terminal.Wpf.Camera;

namespace SP.Parking.Terminal.Wpf.Views
{
    /// <summary>
    /// Interaction logic for CameraView.xaml
    /// </summary>
    public partial class CameraView : BaseView
    {
        SP.Parking.Terminal.Core.Models.Camera _camera;
        public SP.Parking.Terminal.Core.Models.Camera Camera
        {
            get { return _camera; }
            set
            {
                if (_camera == value) return;
                _camera = value;
                var rawCam = _camera.RawCamera;
                switch (_camera.CameraType)
                {
                    case Core.Services.CameraType.Vivotek:
                        Mvx.RegisterType<ICamera, VitaminCamera>();
                        break;
                    case Core.Services.CameraType.Hik:
                        Mvx.RegisterType<ICamera, HIKCamera>();
                        break;
                    case Core.Services.CameraType.Bosch:
                        Mvx.RegisterType<ICamera, BoschCamera>();
                        break;
                    case Core.Services.CameraType.Webcam:
                        Mvx.RegisterType<ICamera, Webcam>();
                        break;
					case Core.Services.CameraType.RTSP:
						Mvx.RegisterType<ICamera, RtspSimple>();
						break;
					case Core.Services.CameraType.VivotekTracker:
					case Core.Services.CameraType.HikTracker:
					case Core.Services.CameraType.RTSPTracker:
					case Core.Services.CameraType.RTSPHDTracker:
						Mvx.RegisterType<ICamera, TrackerCamera>();
						break;
				}
                if (rawCam == null)
                {
                    _camera.Setup(_camera.CameraType, true, true);
                }
                else
                {
                    switch (_camera.CameraType)
                    {
                        case Core.Services.CameraType.Vivotek:
                            if (!(rawCam is VitaminCamera) || _camera.IP != rawCam.IPAddress || _camera.Port != rawCam.Port
                                || _camera.UserName != rawCam.UserName || _camera.Password != rawCam.Password || _camera.WayType != rawCam.WayType)
                            {
                                _camera.Setup(_camera.CameraType, true, true);
                            }

                            break;
                        case Core.Services.CameraType.Hik:
                            if (!(rawCam is HIKCamera) || _camera.IP != rawCam.IPAddress || _camera.Port != rawCam.Port
                                || _camera.UserName != rawCam.UserName || _camera.Password != rawCam.Password || _camera.WayType != rawCam.WayType
                                || _camera.Channel != rawCam.Channel)
                            {
                                _camera.Setup(_camera.CameraType, true, true);
                            }

                            break;
                        case Core.Services.CameraType.Bosch:
                            if (!(rawCam is BoschCamera) || _camera.IP != rawCam.IPAddress || _camera.Port != rawCam.Port
                                || _camera.UserName != rawCam.UserName || _camera.Password != rawCam.Password || _camera.WayType != rawCam.WayType)
                            {
                                _camera.Setup(_camera.CameraType, true, true);
                            }

                            break;
                        case Core.Services.CameraType.Webcam:
                            if (!(rawCam is VitaminCamera) || _camera.IP != rawCam.IPAddress || _camera.Port != rawCam.Port
                                || _camera.UserName != rawCam.UserName || _camera.Password != rawCam.Password || _camera.WayType != rawCam.WayType)
                            {
                                _camera.Setup(_camera.CameraType, true, true);
                            }

                            break;
						case Core.Services.CameraType.RTSP:
							if (!(rawCam is RtspSimple) || _camera.IP != rawCam.IPAddress)
								_camera.Setup(_camera.CameraType, true, true);
							break;
						case Core.Services.CameraType.VivotekTracker:
						case Core.Services.CameraType.HikTracker:
						case Core.Services.CameraType.RTSPTracker:
						case Core.Services.CameraType.RTSPHDTracker:
							if (!(rawCam is TrackerCamera) || _camera.IP != rawCam.IPAddress)
								_camera.Setup(_camera.CameraType, true, true);
							break;
					}
                }
                _camera.RawCamera.ActiveZoom(_camera.Zoomable);
                if (Camera.Container.Parent == null)
                    this.MainGrid.Children.Add(Camera.Container);
                else
                {
                    (Camera.Container.Parent as Panel).Children.Remove(Camera.Container);
                    this.MainGrid.Children.Add(Camera.Container);
                }
				
			}
        }
        Image _imageView;
        public Image ImageView
        {
            get { return _imageView; }
            set
            {
                if (_imageView == value) return;
                _imageView = value;
            }
        }

        private bool _overlay;
        public bool Overlay
        {
            get { return _overlay; }
            set
            {
                _overlay = value;
            }
        }
        BitmapImage _bmpImg = null;

        public CameraView()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            InitializeComponent();
            this.Focusable = true;
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            //_imageView = new Image();
            //this.MainGrid.Children.Add(_imageView);
            //_bmpImg = new BitmapImage();

            //Camera.OnFrameReceived += Camera_OnFrameReceived;
        }
		public override void ViewUnloaded(object sender, RoutedEventArgs e)
		{
			if (_camera != null)
				_camera.Stop();
			base.ViewUnloaded(sender, e);
		}

		//void Camera_OnFrameReceived(object sender, FrameEventArgs e)
		//{
		//    MemoryStream ms = new MemoryStream(e.Frame);

		//    _bmpImg.BeginInit();
		//    _bmpImg.StreamSource = ms;
		//    _bmpImg.EndInit();

		//    ImageSource imgSrc = _bmpImg as ImageSource;

		//    this.ImageView.Source = imgSrc;
		//}

		public override void Close()
        {
			if (_camera != null)
				_camera.Stop();
			System.Threading.Thread.Sleep(5);
			this.MainGrid.Children.Clear();
			base.Close();
			//_camera.Stop();
			//_camera.Dispose();
			//this.MainGrid.Children.Clear();
        }
    }
}
