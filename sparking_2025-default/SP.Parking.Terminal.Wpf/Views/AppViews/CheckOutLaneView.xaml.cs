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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using SP.Parking.Terminal.Core.Services;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Wpf.UI;
using SP.Parking.Terminal.Wpf.Utility;
using System.Media;
using Green.Devices.Dal.CashierCounter;
using System.Globalization;
using System.Windows.Markup;
using System.Threading;
using ControlzEx.Standard;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for CheckOutLaneView.xaml
    /// </summary>
    public partial class CheckOutLaneView : BaseLaneView
    {
        IResourceLocatorService _resourceLocator;

        MediaPlayer _player;
        Uri _uri;

        public NotificationOverlay Notification { get; private set; }
        public OpenBarrierPopupOverlay BarrierNote { get; private set; }
        public CashierPopupOverlay Cashier { get; private set; }

        public new CheckOutLaneViewModel ViewModel
        {
            get { return (CheckOutLaneViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        byte[] _refFontImage;
        public byte[] RefFrontImage
        {
            get { return _refFontImage; }
            set
            {
                if (_refFontImage == value) return;
                if (this.COReferenceFrontImage.Source != null)
                {
                    (COReferenceFrontImage.Source as BitmapImage).StreamSource.Dispose();
                    (COReferenceFrontImage.Source as BitmapImage).StreamSource = null;
                    this.COReferenceFrontImage.Source = null;
                }

                _refFontImage = value;
                ConvertNow(_refFontImage, this.COReferenceFrontImage);
            }
        }

        byte[] _refBackImage;
        public byte[] RefBackImage
        {
            get { return _refBackImage; }
            set
            {
                if (_refBackImage == value) return;
                if (this.COReferenceBackImage.Source != null)
                {
                    (COReferenceBackImage.Source as BitmapImage).StreamSource.Dispose();
                    (COReferenceBackImage.Source as BitmapImage).StreamSource = null;
                    this.COReferenceBackImage.Source = null;
                }

                _refBackImage = value;
                ConvertNow(_refBackImage, this.COReferenceBackImage);
            }
        }

        Section _section;
        public Section Section
        {
            get { return _section; }
            set
            {
                if (_section == value) return;

                _section = value;
                _section = value;
                string _vehicleName = "";
                TypeHelper.GetVehicleType(_section.Lane.VehicleTypeId, result =>
                {
                    if (result != null)
                        _vehicleName = result.Name;
                });

                //MainGroupBox.Header = string.Format("THÔNG TIN XE RA ({0} - {1})", _section.LaneName, _vehicleName);
                Setup();
            }
        }


        private bool _showCountdown;
        public bool ShowCountdown
        {
            get { return _showCountdown; }
            set
            {
                _showCountdown = value;
                CountdownPanel.Visibility = _showCountdown ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
        }

        public object CheckOutData { get; private set; }

        //CheckOut _checkOutData;
        //public CheckOut CheckOutData
        //{
        //    get { return _checkOutData; }
        //    set
        //    {
        //        if (_checkOutData == value) return;

        //        _checkOutData = value;
        //    }
        //}

        public override void DisplayNotices()
        {
            Notification.Notices = this.NoticesToUser;
            Notification.ShowNotification(NoticesToUser.TimeOut);
        }
        IHostSettings _hostSettings;
        public CheckOutLaneView()
        {
            InitializeComponent();

            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            _resourceLocator = Mvx.Resolve<IResourceLocatorService>();
			_hostSettings = Mvx.Resolve<IHostSettings>();
			Notification = new NotificationOverlay()
            {
                Container = CameraGrid,
            };
            BarrierNote = new OpenBarrierPopupOverlay()
            {
                Container = this,
                IsCheckIn = false,
            };
            Cashier = new CashierPopupOverlay()
            {
                Container = this
            };
            _player = new MediaPlayer();
            _uri = new Uri(@"Sounds/checkout.success.mp3", UriKind.Relative);

            //_soundPlayer = new SoundPlayer(@"Sounds/checkout.wav");
            
            //An/Hien nut test he thong
            //btTest.Visibility = Visibility.Collapsed;
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            BindData();
			ViewModel.MainualPlateDoing += new Action<string>(OpenMainualPlate);
            ViewModel.RetailInvoice += new Action<long, long>(OpenInvoicePopup);
			ViewModel.CheckOutCompleted += OnCheckoutCompleted;

            this.FrontCamera.Focusable = true;
            this.FrontCamera.Focus();
        }
        private long pkid, pkfee;
		private void OpenInvoicePopup(long parkingid, long fee)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(async () =>
			{
				pkid = parkingid;
                pkfee = fee;
				Core.TimeAgo.IsDailog = true;
				var dailog = new InvoicePopup();
				dailog.Focusable = true;
				dailog.Focus();
				if (dailog.ShowDialog() == true)
				{
                    string buyer_code = string.IsNullOrEmpty(dailog.BuyerCode)?null : dailog.BuyerCode;
					string buyer_name = string.IsNullOrEmpty(dailog.BuyerName) ? null : dailog.BuyerName;
					string legal_name = string.IsNullOrEmpty(dailog.LegalName) ? null : dailog.LegalName;
					string tax_code = string.IsNullOrEmpty(dailog.TaxCode) ? null : dailog.TaxCode;
					string address = string.IsNullOrEmpty(dailog.Address) ? null : dailog.Address;
					string phone = string.IsNullOrEmpty(dailog.Phone) ? null : dailog.Phone;
					string email = string.IsNullOrEmpty(dailog.Email) ? null : dailog.Email;
					string receiver_name = string.IsNullOrEmpty(dailog.ReceiverName) ? null : dailog.ReceiverName;
					string receiver_emails = string.IsNullOrEmpty(dailog.ReceiverEmails) ? null : dailog.ReceiverEmails;

                    bool has_buyer = (!string.IsNullOrEmpty(receiver_emails) && !string.IsNullOrEmpty(receiver_emails)) || !string.IsNullOrEmpty(buyer_name);
                    ViewModel.CallRetailInvoice(pkid, pkfee, true, has_buyer, buyer_code, buyer_name, legal_name, tax_code, phone, email, address, receiver_name, receiver_emails);
					await Task.Delay(1000);
					Core.TimeAgo.IsDailog = false;
				}
				else
				{
                    ViewModel.CancelCallInvoice();
					await Task.Delay(1000);
					Core.TimeAgo.IsDailog = false;
				}
			});
		}

		public override void ViewUnloaded(object sender, RoutedEventArgs e)
        {
            base.ViewUnloaded(sender, e);

            ViewModel.CheckOutCompleted -= OnCheckoutCompleted;
			ViewModel.MainualPlateDoing -= OpenMainualPlate;
			ViewModel.RetailInvoice -= OpenInvoicePopup;
		}
		private  void OpenMainualPlate(string plate)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(async () =>
			{
				Core.TimeAgo.IsDailog = true;
				var dailog = new CollectPlateToOut($"Lane: {this.ViewModel.Section.LaneName}", plate);
				dailog.Focusable = true;
				dailog.SetForCus();
				dailog.Focus();
				if (dailog.ShowDialog() == true)
				{
					this.ViewModel.PlateCheckOut(dailog.PlateText);
					await Task.Delay(3000);
					Core.TimeAgo.IsDailog = false;
				}
				else
				{
					await Task.Delay(3000);
					Core.TimeAgo.IsDailog = false;
				}
			});
		}
		private void OnCheckoutCompleted(object sender, EventArgs e)
        {
            CheckoutEventArgs checkoutArgs = (CheckoutEventArgs)e;
            if (checkoutArgs.Key == KeyAction.CheckOut)
            {
                if (_player != null)
                {
                    _player.Open(_uri);
                    _player.Play();
                    //_soundPlayer.Play();
                }
            }
        }

        private void Setup()
        {

			//if (this.Section.IsOutExtra)
			//{

			//    if (CameraGrid.ColumnDefinitions.Count < 3)
			//    {
			//        //
			//        ColumnDefinition c = new ColumnDefinition();
			//        c.Width = new GridLength(1, GridUnitType.Star);
			//        CameraGrid.ColumnDefinitions.Add(c);
			//    }
			//    //Grid.SetColumn(groundImg, 2);
			//    Grid.SetColumn(FrontCamera, 1);
			//    Grid.SetColumn(BackCamera, 1);
			//    Grid.SetColumn(COReferenceFrontImage, 2);
			//    //Grid.SetColumn(vc, 2);
			//    Grid.SetColumn(COReferenceBackImage, 2);
			//    //Grid.SetColumn(Extra1Camera, 0);
			//    //Grid.SetColumn(Extra2Camera, 0);
			//    //this.Extra1Camera.Camera = this.Section.ExtraOut1Camera;
			//    //this.Extra2Camera.Camera = this.Section.ExtraOut2Camera;
			//}
			//else
			//{
			//    if (CameraGrid.ColumnDefinitions.Count == 3)
			//        CameraGrid.ColumnDefinitions.RemoveAt(0);
			//    //Grid.SetColumn(groundImg, 1);
			//    Grid.SetColumn(FrontCamera, 0);
			//    Grid.SetColumn(BackCamera, 0);
			//    Grid.SetColumn(COReferenceFrontImage, 1);
			//    //Grid.SetColumn(vc, 1);
			//    Grid.SetColumn(COReferenceBackImage, 1);   
			//}

			if (_hostSettings.ActualSections == 2)
				SetLayOutFour();
			else if (_hostSettings.ActualSections == 1)
				SetLayOutThree();
			else
				SetLayOutTwo();
			this.FrontCamera.Camera = this.Section.FrontOutCamera;
            this.BackCamera.Camera = this.Section.BackOutCamera;
        }

		private void SetLayOutTwo()
		{
			CameraGrid.Children.Clear();
			//Row Grid
			var trowDefinition = new RowDefinition();
			trowDefinition.Height = new GridLength(1, GridUnitType.Star);
			var browDefinition = new RowDefinition();
			browDefinition.Height = new GridLength(1, GridUnitType.Star);

			//Col Grid
			var lcolDefinition = new ColumnDefinition();
			lcolDefinition.Width = new GridLength(1, GridUnitType.Star);
			var rcolDefinition = new ColumnDefinition();
			rcolDefinition.Width = new GridLength(1, GridUnitType.Star);

			CameraGrid.ColumnDefinitions.Add(lcolDefinition);
			CameraGrid.ColumnDefinitions.Add(rcolDefinition);
			CameraGrid.RowDefinitions.Add(trowDefinition);
			CameraGrid.RowDefinitions.Add(browDefinition);
			CameraGrid.Children.Add(imgLogo);
			CameraGrid.Children.Add(imgFont);
			CameraGrid.Children.Add(imgBack);
			CameraGrid.Children.Add(camFont);
			CameraGrid.Children.Add(camBack);

			Grid.SetColumn(imgLogo, 0);
			Grid.SetRow(imgLogo, 0);
			Grid.SetRowSpan(imgLogo, 2);

			Grid.SetColumn(imgFont, 0);
			Grid.SetRow(imgFont, 0);

			Grid.SetColumn(imgBack, 0);
			Grid.SetRow(imgBack, 1);

			Grid.SetColumn(camFont, 1);
			Grid.SetRow(camFont, 0);

			Grid.SetColumn(camBack, 1);
			Grid.SetRow(camBack, 1);
		}

		private void SetLayOutThree()
		{
			CameraGrid.Children.Clear();
			//Row Grid
			var trowDefinition = new RowDefinition();
			trowDefinition.Height = new GridLength(2, GridUnitType.Star);
			var mrowDefinition = new RowDefinition();
			mrowDefinition.Height = new GridLength(2, GridUnitType.Star);
			var browDefinition = new RowDefinition();
			browDefinition.Height = new GridLength(1, GridUnitType.Star);

			//Col Grid
			var lcolDefinition = new ColumnDefinition();
			lcolDefinition.Width = new GridLength(1, GridUnitType.Star);
			var rcolDefinition = new ColumnDefinition();
			rcolDefinition.Width = new GridLength(1, GridUnitType.Star);

			CameraGrid.ColumnDefinitions.Add(lcolDefinition);
			CameraGrid.ColumnDefinitions.Add(rcolDefinition);
			CameraGrid.RowDefinitions.Add(trowDefinition);
			CameraGrid.RowDefinitions.Add(mrowDefinition);
			CameraGrid.RowDefinitions.Add(browDefinition);

			CameraGrid.Children.Add(imgLogo);
			CameraGrid.Children.Add(imgFont);
			CameraGrid.Children.Add(imgBack);
			CameraGrid.Children.Add(camFont);
			CameraGrid.Children.Add(camBack);

			Grid.SetColumn(imgLogo, 0);
			Grid.SetRow(imgLogo, 2);
			Grid.SetColumnSpan(imgLogo, 2);

			Grid.SetColumn(imgFont, 0);
			Grid.SetRow(imgFont, 2);

			Grid.SetColumn(imgBack, 1);
			Grid.SetRow(imgBack, 2);

			Grid.SetColumn(camFont, 0);
			Grid.SetRow(camFont, 0);
			Grid.SetColumnSpan(camFont, 2);

			Grid.SetColumn(camBack, 0);
			Grid.SetRow(camBack, 1);
			Grid.SetColumnSpan(camBack, 2);
		}

		private void SetLayOutFour()
		{
			CameraGrid.Children.Clear();
			//Row Grid
			var trowDefinition = new RowDefinition();
			trowDefinition.Height = new GridLength(1, GridUnitType.Star);
			var browDefinition = new RowDefinition();
			browDefinition.Height = new GridLength(1, GridUnitType.Star);

			//Col Grid
			var lcolDefinition = new ColumnDefinition();
			lcolDefinition.Width = new GridLength(1, GridUnitType.Star);
			var mcolDefinition = new ColumnDefinition();
			mcolDefinition.Width = new GridLength(1.4, GridUnitType.Star);
			var rcolDefinition = new ColumnDefinition();
			rcolDefinition.Width = new GridLength(1.4, GridUnitType.Star);

			CameraGrid.ColumnDefinitions.Add(lcolDefinition);
			CameraGrid.ColumnDefinitions.Add(mcolDefinition);
			CameraGrid.ColumnDefinitions.Add(rcolDefinition);
			CameraGrid.RowDefinitions.Add(trowDefinition);
			CameraGrid.RowDefinitions.Add(browDefinition);

			CameraGrid.Children.Add(imgLogo);
			CameraGrid.Children.Add(imgFont);
			CameraGrid.Children.Add(imgBack);
			CameraGrid.Children.Add(camFont);
			CameraGrid.Children.Add(camBack);

			Grid.SetColumn(imgLogo, 0);
			Grid.SetRow(imgLogo, 0);
			Grid.SetRowSpan(imgLogo, 2);

			Grid.SetColumn(imgFont, 0);
			Grid.SetRow(imgFont, 0);

			Grid.SetColumn(imgBack, 0);
			Grid.SetRow(imgBack, 1);

			Grid.SetColumn(camFont, 1);
			Grid.SetRow(camFont, 0);
			Grid.SetRowSpan(camFont, 2);

			Grid.SetColumn(camBack, 2);
			Grid.SetRow(camBack, 0);
			Grid.SetRowSpan(camBack, 2);
		}
		private void ViewModel_ShowPopupBarrier(object sender, EventArgs e)
        {
            BarrierNote.Visibility = Visibility.Visible;
            BarrierNote.Hidden = false;
        }
        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<CheckOutLaneView, CheckOutLaneViewModel>();
            set.Bind(this).For(v => v.Section).To(vm => vm.Section);
            set.Bind(this).For(v => v.RefFrontImage).To(vm => vm.CheckOutData.ReferenceFrontImage);
            set.Bind(this).For(v => v.RefBackImage).To(vm => vm.CheckOutData.ReferenceBackImage);
            set.Bind(this).For(v => v.ShowCountdown).To(vm => vm.ShowCountdown);
            //set.Bind(this).For(v => v.CheckOutData).To(vm => vm.CheckOutData);
            //set.Bind(this).For(v => ShowChooseVehicleType).To(vm => vm.ShowChooseVehicleType);
            set.Apply();
            ViewModel.ShowPopupBarrier += ViewModel_ShowPopupBarrier;
            //ViewModel.ShowCashier = ShowCashier;
        }

        private void ShowCashier(CashierITC obj)
        {
            if (obj != null)
            {

                ItcCashierWrapper.GetItcCashier(ViewModel.Section.ComIctCashier).TransactionProcess(obj.Info);
                Cashier.Info = obj;
                obj.EndTransaction = EndCashier;
                
                Cashier.Visibility = Visibility.Visible;
               
                Cashier.Hidden = false;
            }
        }

        private void EndCashier(int obj)
        {
            
            //Cashier.Visibility = Visibility.Hidden;
            Cashier.Hidden = true;
            ViewModel.ShowCashAmount(obj);

        }

        public override void Close()
        {
            if (FrontCamera != null)
                this.FrontCamera.Close();
            if (BackCamera != null)
                this.BackCamera.Close();
            //if (Extra1Camera != null)
            //    Extra1Camera.Close();
            //if (Extra2Camera != null)
            //    Extra2Camera.Close();
            COReferenceFrontImage.Source = null;
            COReferenceFrontImage = null;
            COReferenceBackImage.Source = null;
            COReferenceBackImage = null;
            ViewModel.ShowPopupBarrier -= ViewModel_ShowPopupBarrier;
        }

        private void btnAddVoucher_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (ViewModel != null && ViewModel.CheckOutData != null)
            //    {
            //        vc.data = new Voucher();
            //        vc.data.Check_In_Time = ViewModel.CheckOutData.ReferenceCheckInTime;
            //        vc.data.CardId = ViewModel.CheckOutData.CardId;
            //        vc.data.Parking_Fee = ViewModel.CustomerInfo.ParkingFee;
            //        vc.data.Actual_Fee = ViewModel.CustomerInfo.ParkingFee;
            //        vc.data.Voucher_Amount = 0;
            //        vc.data.Voucher_Type = "";
            //        vc.CardIDVC = ViewModel.CardIDVC;
            //        vc.SecondVC = ViewModel.SecondVC;
            //        vc.ShowText();
            //        vc.VehicleTypeId = ViewModel.CheckInVehicle;
            //        vc.CanVoucher = true;
            //        vc.IsVoucher = false;
            //        vc.model = ViewModel;
            //        vc.Visibility = Visibility.Visible;
            //        vc.ResetText();
            //    }
            //    else if (ViewModel != null)
            //    {
            //        vc.IsVoucher = false;
            //        vc.CanVoucher = false;
            //        vc.Visibility = Visibility.Hidden;
            //    }
            //}
            //catch
            //{
            //    vc.IsVoucher = false;
            //    vc.CanVoucher = false;
            //    vc.Visibility = Visibility.Hidden;
            //}
        }

        //private void button_Click(object sender, RoutedEventArgs e)
        //{
        //   ViewModel.ReadingCompleted(sender, new Green.Devices.Dal.CardReaderEventArgs { CardID = "A640FF0F" });
        // }
        int i = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.GreenReadingCompleted(sender, new Green.Devices.Dal.GreenCardReaderEventArgs
            {
                CardID = "55AA2233",//"990A2919","6C1D54A6"
                Time = DateTime.Now
            });
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.AutoCheckOutCommand.Execute(txt_vehicle_number.Text);
            }
        }
    }
}
