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
using System.Windows.Forms.Integration;
using SP.Parking.Terminal.Wpf.UI;
using Cirrious.MvvmCross.ViewModels;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for CheckInLaneView.xaml
    /// </summary>
    public partial class CheckInLaneView : BaseLaneView
    {
        IResourceLocatorService _resourceLocator;

        public NotificationOverlay Notification { get; private set; }
        public OpenBarrierPopupOverlay BarrierNote { get; private set; }

        public new CheckInLaneViewModel ViewModel
        {
            get { return (CheckInLaneViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        byte[] _fontImage;
        public byte[] FrontImage
        {
            get { return _fontImage; }
            set
            {
                if (_fontImage == value) return;
                if (this.CIFrontImage.Source != null)
                {
                    (CIFrontImage.Source as BitmapImage).StreamSource.Dispose();
                    (CIFrontImage.Source as BitmapImage).StreamSource = null;
                    this.CIFrontImage.Source = null;
                }

                _fontImage = value;
                ConvertNow(_fontImage, this.CIFrontImage);
            }
        }

        byte[] _backImage;
        public byte[] BackImage
        {
            get { return _backImage; }
            set
            {
                if (_backImage == value) return;
                if (this.CIBackImage.Source != null)
                {
                    (CIBackImage.Source as BitmapImage).StreamSource.Dispose();
                    (CIBackImage.Source as BitmapImage).StreamSource = null;
                    this.CIBackImage.Source = null;
                }

                _backImage = value;
                ConvertNow(_backImage, this.CIBackImage);
            }
        }

        //BitmapImage _biImg = new BitmapImage();

        //private CheckIn _
        //;
        //public CheckIn CheckInData
        //{
        //    get { return _checkInData; }
        //    set
        //    {
        //        if (_checkInData == value) 
        //            return;

        //        _checkInData = value;


        //        //this.VehicleTextBox.ShouldFocuse = true;
        //    }
        //}

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

        private bool _showEntries;
        public bool ShowEntries
        {
            get { return _showEntries; }
            set
            {
                _showEntries = value;
                if (_showEntries && _settings.EntryCheck)
                {
                    var entryCheck = new EntryCheck
                    {
                        CheckInData = ViewModel.CheckInData
                    };
                    entryCheck.ShowDialog();
                }

                _showEntries = false;
            }
        }

        public override void DisplayNotices()
        {
            Notification.Notices = this.NoticesToUser;
            Notification.ShowNotification(NoticesToUser.TimeOut);
        }

        Section _section;
        private IOptionsSettings _settings;
        private IHostSettings _hostSettings;
        public Section Section
        {
            get { return _section; }
            set
            {
                if (_section == value) return;
                _section = value;
                string _vehicleName = "";
                TypeHelper.GetVehicleType(_section.Lane.VehicleTypeId, result =>
                {
                    if (result != null)
                        _vehicleName = result.Name;
                });
                //MainGroupBox.Header = string.Format("THÔNG TIN XE VÀO ({0} - {1}) ", _section.LaneName, _vehicleName);
                Setup();
            }
        }

        public CheckInLaneView()
        {
            InitializeComponent();

            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            _resourceLocator = Mvx.Resolve<IResourceLocatorService>();
            _settings = Mvx.Resolve<IOptionsSettings>();
			_hostSettings = Mvx.Resolve<IHostSettings>();
			
			Notification = new NotificationOverlay()
            {
                Container = CameraGrid,
            };
            BarrierNote = new OpenBarrierPopupOverlay()
            {
                Container = this,
                IsCheckIn = true,
            };

            //An/Hien nut test he thong
            //btTest.Visibility = Visibility.Collapsed;
        }

        private void ViewModel_ShowPopupBarrier(object sender, EventArgs e)
        {
            BarrierNote.Visibility = Visibility.Visible;
            BarrierNote.Hidden = false;
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            BindData();
			ViewModel.MainualPlateDoing += new Action<string>(OpenMainualPlate);
			this.FrontCamera.Focusable = true;
            this.FrontCamera.Focus();
        }
		public override void ViewUnloaded(object sender, RoutedEventArgs e)
		{
			base.ViewUnloaded(sender, e);
			ViewModel.MainualPlateDoing -= OpenMainualPlate;
		}
		private void OpenMainualPlate(string plate)
		{
			System.Windows.Application.Current.Dispatcher.Invoke(async () =>
			{
				Core.TimeAgo.IsDailog = true;
				var dailog = new CollectPlateToIn($"Lane: {this.ViewModel.Section.LaneName}", plate);
				dailog.Focusable = true;
				dailog.SetForCus();
				dailog.Focus();
				if (dailog.ShowDialog() == true)
				{
					this.ViewModel.PlateCheckIn(dailog.PlateText);
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
		private void Setup()
        {

            //if (this.Section.IsInExtra)
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
            //    Grid.SetColumn(CIFrontImage, 2);
            //    Grid.SetColumn(CIBackImage, 2);
            //    //Grid.SetColumn(Extra1Camera, 0);
            //    //Grid.SetColumn(Extra2Camera, 0);
            //    //this.Extra1Camera.Camera = this.Section.ExtraIn1Camera;
            //    //this.Extra2Camera.Camera = this.Section.ExtraIn2Camera;
            //}
            //else
            //{
            //    if (CameraGrid.ColumnDefinitions.Count == 3)
            //        CameraGrid.ColumnDefinitions.RemoveAt(0);
            //    Grid.SetColumn(FrontCamera, 0);
            //    Grid.SetColumn(BackCamera, 0);
            //    //Grid.SetColumn(groundImg, 1);
            //    Grid.SetColumn(CIFrontImage, 1);
            //    Grid.SetColumn(CIBackImage, 1);
            //}
            if (_hostSettings.ActualSections == 2)
                SetLayOutFour();
            else if (_hostSettings.ActualSections == 1)
                SetLayOutThree();
            else
                SetLayOutTwo();

			this.FrontCamera.Camera = this.Section.FrontInCamera;
            this.BackCamera.Camera = this.Section.BackInCamera;
            Section.FrontInCamera.WayType = Section.BackInCamera.WayType = "IN";
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
		public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<CheckInLaneView, CheckInLaneViewModel>();
            set.Bind(this).For(v => v.Section).To(vm => vm.Section);
            //set.Bind(this).For(v => v.CheckInData).To(vm => vm.CheckInData);
            set.Bind(this).For(v => v.FrontImage).To(vm => vm.CheckInData.FrontImage);
            set.Bind(this).For(v => v.BackImage).To(vm => vm.CheckInData.BackImage);
            set.Bind(this).For(v => v.ShowCountdown).To(vm => vm.ShowCountdown);
            set.Bind(this).For(v => v.ShowEntries).To(vm => vm.ShowEntries);
            set.Apply();
            ViewModel.ShowPopupBarrier += ViewModel_ShowPopupBarrier;
            ViewModel.ShowAddRegisteredCardWindow += ViewModel_ShowAddRegisteredCardWindow;
        }

        private void ViewModel_ShowAddRegisteredCardWindow(object sender, EventArgs e)
        {
            var addRegisteredCard = new AddRegisteredCard()
            {
                Owner  = this.TryFindParent<MainWindow>(),
                Title = "Mapping Card",
                CardId = ViewModel.CheckedCard.Id
            };

            addRegisteredCard.MapCardSuccess += (arg1, arg2) =>
            {
                //_viewDispatcher.ShowViewModel(new MvxViewModelRequest(typeof(ConfigViewModel), null, null, null));
            };
            addRegisteredCard.ShowDialog();
        }

        public override void Close()
        {
            if (FrontCamera != null)
                FrontCamera.Close();
            if (BackCamera != null)
                BackCamera.Close();
            //if (Extra1Camera != null)
            //    Extra1Camera.Close();
            //if (Extra2Camera != null)
            //    Extra2Camera.Close();
            if (CIFrontImage != null)
            {
                this.CIFrontImage.Source = null;
                this.CIFrontImage = null;
            }

            if (CIBackImage != null)
            {
                this.CIBackImage.Source = null;
                this.CIBackImage = null;
            }
            ViewModel.ShowPopupBarrier -= ViewModel_ShowPopupBarrier;
        }

        public override bool InterceptCloseViewRequest(BaseViewModel childVM)
        {


            return base.InterceptCloseViewRequest(childVM);
        }

        private void ToggleFlyout(int index)
        {
            var flyout = this.MainWindow.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //5CB240A6 Charm
            ViewModel.GreenReadingCompleted(sender, new Green.Devices.Dal.GreenCardReaderEventArgs
            {
                CardID = "55AA2233",//"990A2919","6C1D54A6"
                Time = DateTime.Now
            });

            //StartOtherApp();
        }

        private void StartOtherApp()
        {
            try {
                FileInfo spectrumFileInfo = new FileInfo("C:\\Users\\dinht\\Downloads\\GetDoubleCheckIn\\InParkExport\\bin\\Debug\\ParkingUt.exe");
                ProcessStartInfo info = new ProcessStartInfo(spectrumFileInfo.FullName);
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.WorkingDirectory = spectrumFileInfo.DirectoryName;
                Process pVizualizer = new Process();
                pVizualizer.StartInfo = info;
                pVizualizer.EnableRaisingEvents = true;
                pVizualizer.Exited += new EventHandler(myProcess_Exited);
                pVizualizer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            Console.WriteLine(
                $"Exit time    : \n" +
                $"Exit code    : \n" +
                $"output    : \n" +
                $"err    : \n"
                );
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.AutoCheckInCommand.Execute(txt_vehicle_number.Text);
            }
        }
    }
}