using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.CrossCore;
using Green.Devices.Dal;
using SP.Parking.Terminal.Wpf.Devices;
using SP.Parking.Terminal.Core.Services;
using System.Threading;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for SubLaneConfigurationView.xaml
    /// </summary>
    public partial class SubLaneConfigurationView : BaseView
    {
        private const string REMOTE_CARD_READER = "Remote Card Reader";

        public new SubLaneConfigurationViewModel ViewModel
        {
            get { return (SubLaneConfigurationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        bool _isDetecting;
        public bool IsDetecting
        {
            get { return _isDetecting; }
            set
            {
                _isDetecting = value;
                if(_isDetecting)
                    this.DetectButton.Background = Brushes.Red;
                else
                    this.DetectButton.SetResourceReference(Control.BackgroundProperty, "AccentColorBrush");
            }
        }

        string _currentCardReader;
        public string CurrentCardReader
        {
            get { return _currentCardReader; }
            set
            {
                _currentCardReader = value;
                UpdateIpVisibility();
            }
        }

        string _cardReaderIp;

        public string CardReaderIp
        {
            get { return _cardReaderIp; }
            set
            {
                _cardReaderIp = value;
                UpdateIpVisibility();
            }
        }

        private void UpdateIpVisibility()
        {
            var show = (CurrentCardReader != null && CurrentCardReader.Equals(REMOTE_CARD_READER))
                || !string.IsNullOrWhiteSpace(CardReaderIp);
            var visibility = show ? Visibility.Visible : Visibility.Hidden;
            IpTextBlock.Visibility = IpTextBox.Visibility = visibility;
        }

        public SubLaneConfigurationView()
        {
            InitializeComponent();
        }
        public override void Close()
        {
            //if (ZoomCamera != null && ZoomCamera.Children.Count == 1)
            //{
            //    CameraView cv = ZoomCamera.Children[0] as CameraView;
            //    cv.Dispose();
            //    ZoomCamera.Children.Clear();
            //}
        }
        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            ViewModel.GetCardReaderInfo();
            if (Mvx.Resolve<ICamera>() is Webcam)
                ViewModel.GetWebcamInfo();
            BindData();
            //CameraView cv = new CameraView();
            //cv.Camera= ViewModel.CarmeraForZoom;
            //ZoomCamera.Children.Add(cv);   
            //cv.Camera.Start();
            UpdateIpVisibility();
           
        }

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<SubLaneConfigurationView, SubLaneConfigurationViewModel>();
            set.Bind(this).For(v => v.IsDetecting).To(vm => vm.IsDetecting);
            set.Bind(this).For(v => v.CurrentCardReader).To(vm => vm.CurrentCardReader);
            set.Bind(this).For(v => v.CardReaderIp).To(vm => vm.CardReaderIP);
            set.Apply();
        }

        private void RemoveCardReader_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var cardReader = button.DataContext as CardReaderWrapper;
            if (cardReader != null)
                ViewModel.RemoveCardReader.Execute(cardReader);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var cardReader = button.DataContext;
            if (cardReader != null)
                ViewModel.RemoveGreenCard.Execute(cardReader);
        }

        private void CardTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            var sl = cb.SelectedIndex;
            if (sl == 0)
            {
                ToggleNFCCard(false);
                ToggleModWinsCard(true);
                ToggleTcpCard(false, sl);
                ToggleProxiesCard(false);
            }
            else if (cb.SelectedValue.Equals("NFC"))
            {
                ToggleNFCCard(true);
                ToggleModWinsCard(false);
                ToggleTcpCard(false, sl);
                ToggleProxiesCard(false);
            }
            else if (cb.SelectedValue.Equals("Proxies"))
            {
                ToggleNFCCard(false);
                ToggleModWinsCard(false);
                ToggleTcpCard(false, sl);
                ToggleProxiesCard(true);
            }
            else if (cb.SelectedValue.Equals("ZKFarCard"))
            {
                ToggleNFCCard(false);
                ToggleModWinsCard(false);
                ToggleTcpCard(true, 3);
                ToggleProxiesCard(false);
            }
            else
            {
                ToggleNFCCard(false);
                ToggleModWinsCard(false);
                ToggleTcpCard(true, sl);
                ToggleProxiesCard(false);
            }
        }

        private void ToggleProxiesCard(bool flag)
        {
            AvailableProxiesCardReader.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleNFCCard(bool flag)
        {
            AvailableNFCReader.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleModWinsCard(bool flag)
        {
            AvilableModWinsComboBox.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
            btnRefreshModWinsCard.Visibility = flag ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleTcpCard(bool flag, int sl = 0)
        {
            if(flag)
            {
                tcpippannel.Visibility = Visibility.Visible;
                if (sl == 3)
                {
                    cbAntenna.Visibility = Visibility.Hidden;
                    cbDoor.Visibility = Visibility.Visible;
                    cbReader.Visibility = Visibility.Visible;
                }
                else if (sl == 5)
                {
                    cbDoor.Visibility = Visibility.Hidden;
                    cbReader.Visibility = Visibility.Hidden;
                    cbAntenna.Visibility = Visibility.Visible;
                }
                else
                {
                    cbAntenna.Visibility = Visibility.Hidden;
                    cbDoor.Visibility = Visibility.Hidden;
                    cbReader.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                tcpippannel.Visibility = Visibility.Hidden;
                cbAntenna.Visibility = Visibility.Hidden;
                cbDoor.Visibility = Visibility.Hidden;
                cbReader.Visibility = Visibility.Hidden;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            //try {
            //    if (ZoomCamera != null && ZoomCamera.Children.Count == 1)
            //    {
            //        CameraView cv = ZoomCamera.Children[0] as CameraView;
            //        cv.Camera.RawCamera.OnZoomReceived -= Cmr_OnZoomReceived;
            //        cv.Dispose();
            //        ZoomCamera.Children.Clear();
            //    }
            //    CameraView cv1 = new CameraView();  
            //    var cc = ViewModel.ZoomList[ViewModel.SelectedZoom];
            //    ZoomFactor zz=new ZoomFactor();
            //    Camera cmr = new Camera();      
            //    switch (cc.Value)
            //    {
            //        case "frontin":
            //            zz = ViewModel.FrontInZoom;
            //            cmr.IP = ViewModel.FrontInCamera;
            //            break;
            //        case "frontout":
            //            zz = ViewModel.FrontOutZoom;
            //            cmr.IP = ViewModel.FrontOutCamera;
            //            break;
            //        case "backin":
            //            zz = ViewModel.BackInZoom;
            //            cmr.IP = ViewModel.BackInCamera;
            //            break;
            //        case "backout":
            //            zz = ViewModel.BackOutZoom;
            //            cmr.IP = ViewModel.BackOutCamera;
            //            break;
            //        case "extra1in":
            //            zz = ViewModel.ExtraIn1Zoom;
            //            cmr.IP = ViewModel.ExtraIn1Camera;
            //            break;
            //        case "extra1out":
            //            zz = ViewModel.ExtraOut1Zoom;
            //            cmr.IP = ViewModel.ExtraOut1Camera;
            //            break;
            //        case "extra2in":
            //            zz = ViewModel.ExtraIn2Zoom;
            //            cmr.IP = ViewModel.ExtraIn2Camera;
            //            break;
            //        case "extra2out":
            //            zz = ViewModel.ExtraOut2Zoom;
            //            cmr.IP = ViewModel.ExtraOut2Camera;
            //            break;
            //    }
            //    if (zz != null)
            //        zz = new ZoomFactor() { Factor = zz.Factor, ZoomX = zz.ZoomX, ZoomY = zz.ZoomY, ZoomEnabled = true };
            //    else
            //        zz = new ZoomFactor()
            //        {
            //            Factor = 100,
            //            ZoomX = 0,
            //            ZoomY = 0,
            //            ZoomEnabled = true
            //        };
            //    cmr.ZoomFactor = zz;
            //    IOptionsSettings _optionSettings = Mvx.Resolve<IOptionsSettings>();
            //    cmr.Setup(_optionSettings.CameraType, true, true);
                
            //    cmr.RawCamera.OnZoomReceived += Cmr_OnZoomReceived;
            //    cv1.Camera = cmr;
            //    ZoomCamera.Children.Add(cv1);
            //    cv1.Camera.Start();
            //    txtFactor.Text = string.Format("Factor: {0}", zz.Factor);
            //    txtZoomx.Text = string.Format("ZoomX: {0}", zz.ZoomX);
            //    txtZoomy.Text = string.Format("ZoomY: {0}", zz.ZoomY);
            //}
            //catch(Exception ex)
            //{
               
            //}
           
           
        }

        private void Badged_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void rdB_WayIn_Checked(object sender, RoutedEventArgs e)
        {
            if (rdB_WayIn.IsChecked == true)
            {
                ipcamewayin1.IsEnabled = true;
                portcamewayin1.IsEnabled = true;
                usercamewayin1.IsEnabled = true;
                passcamewayin1.IsEnabled = true;
                typecamewayin1.IsEnabled = true;

                ipcamewayin2.IsEnabled = true;
                portcamewayin2.IsEnabled = true;
                usercamewayin2.IsEnabled = true;
                passcamewayin2.IsEnabled = true;
                typecamewayin2.IsEnabled = true;

                ipcamewayout1.IsEnabled = false;
                portcamewayout1.IsEnabled = false;
                usercamewayout1.IsEnabled = false;
                passcamewayout1.IsEnabled = false;
                typecamewayout1.IsEnabled = false;

                ipcamewayout2.IsEnabled = false;
                portcamewayout2.IsEnabled = false;
                usercamewayout2.IsEnabled = false;
                passcamewayout2.IsEnabled = false;
                typecamewayout2.IsEnabled = false;

                gWayOut.Visibility = Visibility.Collapsed;
                gWayIn.Visibility = Visibility.Visible;

                ipcamewayin1.Focus();
            }
        }

        private void rdB_WayOut_Checked(object sender, RoutedEventArgs e)
        {
            if (rdB_WayOut.IsChecked == true)
            {
                ipcamewayin1.IsEnabled = false;
                portcamewayin1.IsEnabled = false;
                usercamewayin1.IsEnabled = false;
                passcamewayin1.IsEnabled = false;
                typecamewayin1.IsEnabled = false;

                ipcamewayin2.IsEnabled = false;
                portcamewayin2.IsEnabled = false;
                usercamewayin2.IsEnabled = false;
                passcamewayin2.IsEnabled = false;
                typecamewayin2.IsEnabled = false;

                ipcamewayout1.IsEnabled = true;
                portcamewayout1.IsEnabled = true;
                usercamewayout1.IsEnabled = true;
                passcamewayout1.IsEnabled = true;
                typecamewayout1.IsEnabled = true;

                ipcamewayout2.IsEnabled = true;
                portcamewayout2.IsEnabled = true;
                usercamewayout2.IsEnabled = true;
                passcamewayout2.IsEnabled = true;
                typecamewayout2.IsEnabled = true;

                gWayOut.Visibility = Visibility.Visible;
                gWayIn.Visibility = Visibility.Collapsed;

                ipcamewayout1.Focus();
            }
        }

        //private void Cmr_OnZoomReceived(object sender, ZoomEventArgs e)
        //{
        //    if (ZoomCamera != null && ZoomCamera.Children.Count == 1)
        //    {
        //        CameraView cv = ZoomCamera.Children[0] as CameraView;
        //        cv.Camera.ZoomFactor = e.ZoomFactor;
        //    }
        //}
        //void ChangeZoom()
        //{
        //    try
        //    {
        //        if (ZoomCamera != null && ZoomCamera.Children.Count == 1)
        //        {
        //            CameraView cv = ZoomCamera.Children[0] as CameraView;
        //            cv.Camera.RawCamera.SaveZoomState();

        //            ZoomFactor zz = new ZoomFactor() { Factor = cv.Camera.ZoomFactor.Factor, ZoomEnabled = true, ZoomX = cv.Camera.ZoomFactor.ZoomX, ZoomY = cv.Camera.ZoomFactor.ZoomY };
        //            var cc = ViewModel.ZoomList[ViewModel.SelectedZoom];
        //            switch (cc.Value)
        //            {
        //                case "frontin":
        //                    ViewModel.FrontInZoom = zz;
        //                    break;
        //                case "frontout":
        //                    ViewModel.FrontOutZoom = zz;
        //                    break;
        //                case "backin":
        //                    ViewModel.BackInZoom = zz;
        //                    break;
        //                case "backout":
        //                    ViewModel.BackOutZoom = zz;
        //                    break;
        //                case "extra1in":
        //                    ViewModel.ExtraIn1Zoom = zz;
        //                    break;
        //                case "extra1out":
        //                    ViewModel.ExtraOut1Zoom = zz;
        //                    break;
        //                case "extra2in":
        //                    ViewModel.ExtraIn2Zoom = zz;
        //                    break;
        //                case "extra2out":
        //                    ViewModel.ExtraOut2Zoom = zz;
        //                    break;
        //            }
        //            txtFactor.Text = string.Format("Factor: {0}", zz.Factor);
        //            txtZoomx.Text = string.Format("ZoomX: {0}", zz.ZoomX);
        //            txtZoomy.Text = string.Format("ZoomY: {0}", zz.ZoomY);
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    //ChangeZoom();

        //}
    }
}
