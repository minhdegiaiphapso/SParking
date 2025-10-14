using SP.Parking.Terminal.Wpf.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MahApps.Metro.Controls;
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
using System.Windows.Shapes;
using SP.Parking.Terminal.Wpf.UI;
using System.ComponentModel;
using System.Windows.Threading;
using Cirrious.MvvmCross.Plugins.Messenger;
using SP.Parking.Terminal.Core.Services;
using System.Reflection;
using SP.Parking.Terminal.Wpf.Views.AppViews;
using Cirrious.MvvmCross.Views;
using SP.Parking.Terminal.Core.ViewModels;
using System.Diagnostics;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using SP.Parking.Terminal.Core.Models;
using System.Windows.Forms;

namespace SP.Parking.Terminal.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        //public new static readonly DependencyProperty FlyoutsProperty = DependencyProperty.Register("Flyouts", typeof(FlyoutsControl), typeof(MainWindow), new PropertyMetadata(null));
        //public new FlyoutsControl Flyouts
        //{
        //    get { return (FlyoutsControl)GetValue(FlyoutsProperty); }
        //    set { SetValue(FlyoutsProperty, value); }
        //}

        private IMvxViewDispatcher _viewDispatcher;

        IMvxMessenger _keyPressedMessenger;
        DispatcherTimer _autocardrefreshtimer;
        DispatcherTimer _timer;
       
        DispatcherTimer _longtimer;

        SystemNotificationOverlay _systemNotification;

        ILocalizeService _localeService;

        IUserPreferenceService _userPreferenceService;

        MvxSubscriptionToken _changeServerToken;

        IWebClient _webClient;

        bool _switchServerManually = false;

        public MainWindow()
        {
            InitializeComponent();

            this.StateChanged += (sender, e) =>
            {
                this.MainView.Focus();
            };



            //this.Background = Theme.Theme.Background;
            //this.Topmost = true;
            //this.IgnoreTaskbarOnMaximize = true;

            _systemNotification = new SystemNotificationOverlay()
            {
                Container = this
            };

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
            _timer.Start();
            _longtimer = new DispatcherTimer();
            _longtimer.Interval = TimeSpan.FromMinutes(15);
            _longtimer.Tick += _longtimer_Tick;
            _longtimer.Start();
            _autocardrefreshtimer = new DispatcherTimer();
            _autocardrefreshtimer.Interval = TimeSpan.FromSeconds(5);
            _autocardrefreshtimer.Tick += _autocardrefreshtimer_Tick;
            _autocardrefreshtimer.Start();
            //lblVersion.Content = "Ver: " + OtherUtilities.GetVersion();

            this.KeyUp += MainWindow_KeyUp;
        }
        private int RefreshCardsAfterSeconds;
        void AutoRefreshCards()
        {
            var autotime = _userPreferenceService.OptionsSettings.AutoRefreshTime;
            if (!refresh_doing)
            {
                Task.Factory.StartNew(() => RefreshCards());
            }
            if (RefreshCardsAfterSeconds != autotime)
            {
                RefreshCardsAfterSeconds = autotime;
                _autocardrefreshtimer.Stop();
                if (RefreshCardsAfterSeconds > 0)
                    _autocardrefreshtimer.Interval = TimeSpan.FromSeconds(RefreshCardsAfterSeconds);
                else
                    _autocardrefreshtimer.Interval = TimeSpan.FromSeconds(5);
                _autocardrefreshtimer.Start();
            }
        }
        private void _autocardrefreshtimer_Tick(object sender, EventArgs e)
        {
            //AutoRefreshCards();
        }
        private void _longtimer_Tick(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => TimeMapInfo.Current = _userPreferenceService.GetServerTime());
            //Task.Factory.StartNew(() => CurrentListCardReader.RefreshListCard());
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //lblTime.Content = DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt");
            TimeMapInfo.Current.LocalTime = TimeMapInfo.Current.LocalTime.AddSeconds(1);
            lblTime.Content = TimeMapInfo.Current.LocalTime.ToString("dd/MM/yyyy  hh:mm:ss");
            TimeMapInfo.Current.UtcTime = TimeMapInfo.Current.UtcTime.AddSeconds(1);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MsgBox.Show("BẠN THẬT SỰ MUỐN THOÁT CHƯƠNG TRÌNH?", "THOÁT", MsgBox.Buttons.YesNo, MsgBox.Icon.Question);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                ///2018-08-09
                //_userPreferenceService.SystemSettings.ForceSave();
                Task.Factory.StartNew(() => CurrentListCardReader.RemoveCards());
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        void LoadGreenCardReader()
        {
            var sessions = _userPreferenceService.SystemSettings.GetAllSections();
            foreach (var sec in sessions)
            {
                if (sec.ModWinsCards != null)
                {
                    foreach (var info in sec.ModWinsCards)
                    {
                        CurrentListCardReader.AddCardInfo(info);
                    }
                }
                if (sec.TcpIpServerCards != null)
                {
                    foreach (var info in sec.TcpIpServerCards)
                    {
                        CurrentListCardReader.AddCardInfo(info);
                    }
                }
                if (sec.TcpIpClientCards != null)
                {
                    foreach (var info in sec.TcpIpServerCards)
                    {
                        CurrentListCardReader.AddCardInfo(info);
                    }
                }
                if (sec.TcpIpRemodeCards != null)
                {
                    foreach (var info in sec.TcpIpRemodeCards)
                    {
                        CurrentListCardReader.AddCardInfo(info);
                    }
                }
                if (sec.TcpIpControllerCards != null)
                {
                    foreach (var info in sec.TcpIpControllerCards)
                    {
                        CurrentListCardReader.AddCardInfo(info);
                    }
                }
            }
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
            _keyPressedMessenger = Mvx.Resolve<IMvxMessenger>();
            _localeService = Mvx.Resolve<ILocalizeService>();
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _webClient = Mvx.Resolve<IWebClient>();
            RefreshCardsAfterSeconds = _userPreferenceService.OptionsSettings.AutoRefreshTime;
            _autocardrefreshtimer.Stop();
            if (RefreshCardsAfterSeconds > 0)
                _autocardrefreshtimer.Interval = TimeSpan.FromSeconds(RefreshCardsAfterSeconds);
            else
                _autocardrefreshtimer.Interval = TimeSpan.FromSeconds(5);
            _autocardrefreshtimer.Start();
            
            this.Title = string.IsNullOrEmpty(_userPreferenceService.HostSettings.ParkingName) ? Title : _userPreferenceService.HostSettings.ParkingName;
            //this.Title += " - " + OtherUtilities.GetVersion();
            
            var messenger = Mvx.Resolve<IMvxMessenger>();
            Task.Factory.StartNew(()=> TimeMapInfo.Current = _userPreferenceService.GetServerTime());
            Task.Factory.StartNew(() => { LoadGreenCardReader(); CurrentListCardReader.RefreshListCard(); });
            
            _changeServerToken = messenger.Subscribe<ChangeServerMessage>(msg =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (msg.IsUseSecondary && !_switchServerManually)
                    {
                        _systemNotification.Text = _localeService.GetText("system.primary_server_unreachable");
                        _systemNotification.Show();
                    }
                    else
                    {
                        _systemNotification.Hide();
                    }

                    _switchServerManually = false;
                    SwitchServer(msg.IsUseSecondary);

                });
            });

            primaryServer.IsCheckable = true;
            primaryServer.IsChecked = true;
        }

        void SwitchServer(bool IsUseSecondary)
        {
            //if (IsUseSecondary)
            //{
            //    this.secondaryServer.FontWeight = FontWeights.Bold;
            //    this.secondaryServer.FontSize = 20;
            //    this.secondaryServer.Content = (this.secondaryServer.Content as string).TrimEnd('*');
            //    this.secondaryServer.Content += "*";

            //    this.primaryServer.FontWeight = FontWeights.Normal;
            //    this.primaryServer.FontSize = 14;
            //    this.primaryServer.Content = (this.primaryServer.Content as string).TrimEnd('*');
            //}
            //else
            //{
            //    this.primaryServer.FontWeight = FontWeights.Bold;
            //    this.primaryServer.FontSize = 20;
            //    this.primaryServer.Content = (this.primaryServer.Content as string).TrimEnd('*');
            //    this.primaryServer.Content += "*";

            //    this.secondaryServer.FontWeight = FontWeights.Normal;
            //    this.secondaryServer.FontSize = 14;
            //    this.secondaryServer.Content = (this.secondaryServer.Content as string).TrimEnd('*');
            //}
        }

        void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //switch (e.Key)
            //{
            //    case Key.F12:
            //        {
            //            ButtonConfigClick(null, null);
            //            break;
            //        }
            //    case Key.F11:
            //        {
            //            ButtonFindClick(null, null);
            //            break;
            //        }
            //    case Key.F9:
            //        ButtonStatisticsClick(null, null);
            //        break;
            //}

            if (_keyPressedMessenger.HasSubscriptionsFor<KeyPressedMessage>())
            {
                _keyPressedMessenger.Publish(new KeyPressedMessage(sender, e));
            }
        }

        public void PresentInRegion(BaseView frameworkElement, string regionName)
        {
            this.MainView.Children.Add(frameworkElement);
        }

        public void ShowInMainView(BaseView frameworkElement)
        {
            this.MainView.Children.Clear();
            frameworkElement.MainWindow = this;
            this.MainView.Children.Add(frameworkElement);
        }

        public void CloseCurrentView()
        {
            int count = this.MainView.Children.Count;
            if (count > 0)
                this.MainView.Children.RemoveAt(count - 1);
        }

        

        private void ButtonFindClick(object sender, RoutedEventArgs e)
        {

            //var passwordWindow = new PasswordWindow()
            //{
            //    Owner = this,
            //    Title = "Nhập mật mã để cấu hình",
            //};

            //passwordWindow.AuthenticateSuccess += (arg1, arg2) => {
            var findImagesWindow = new FindImagesWindow()
            {
                Owner = this,
                Title = "",
            };
            findImagesWindow.ShowDialog();
            //};

            //passwordWindow.ShowDialog();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutView aboutView = new AboutView() { Container = this };
            aboutView.ShowDialog();
        }
        private void ButtonStatisticsClick(object sender, RoutedEventArgs e)
        {
            //var passwordWindow = new PasswordWindow()
            //{
            //    Owner = this,
            //    Title = "Nhập mật mã để xem thống kê",
            //};

            //passwordWindow.AuthenticateSuccess += (arg1, arg2) => {
            var statisticsWindow = new StatisticsWindow()
            {
                Owner = this,
                Title = "",
            };
            statisticsWindow.ShowDialog();
            //};

            //passwordWindow.ShowDialog();
        }

        private void primaryServer_Click(object sender, RoutedEventArgs e)
        {
            _switchServerManually = true;
            _webClient.SwitchToPrimaryServer(null, null);

            primaryServer.IsCheckable = true;
            primaryServer.IsChecked = true;
            secondaryServer.IsCheckable = false;
            secondaryServer.IsChecked = false;
        }

        private void secondaryServer_Click(object sender, RoutedEventArgs e)
        {
            _switchServerManually = true;
            _webClient.StartUseSecondaryServer();

            primaryServer.IsCheckable = false;
            primaryServer.IsChecked = false;
            secondaryServer.IsCheckable = true;
            secondaryServer.IsChecked = true;
        }
        private void Refreshcardreader_Click(object sender, RoutedEventArgs e)
        {
            MsgBox.Show(CurrentListCardReader.RefreshListCard(), "Notification", MsgBox.Buttons.OK, MsgBox.Icon.Info);
        }
        
        private bool refresh_doing = false;
        
        void RefreshCards()
        {
            if (!refresh_doing)
            {
                refresh_doing = true;
                CurrentListCardReader.RefreshListCard();
                refresh_doing = false;
            }
        }

        private void MetroWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.U && !refresh_doing)
                Task.Factory.StartNew(() => RefreshCards());
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            var passwordWindow = new PasswordWindow()
            {
                Owner = this,
                Title = "OPEN CONFIGURATION",
            };
            passwordWindow.AuthenticateSuccess += (arg1, arg2) =>
            {
                _viewDispatcher.ShowViewModel(new MvxViewModelRequest(typeof(ConfigViewModel), null, null, null));
            };
            passwordWindow.ShowDialog();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            AboutView aboutView = new AboutView() { Container = this };
            aboutView.ShowDialog();
        }

        //private void grdCam_Loaded(object sender, RoutedEventArgs e)
        //{
        //    System.Windows.Forms.Panel pnl = new System.Windows.Forms.Panel();
        //    Bosch.VideoSDK.AxCameoLib.AxCameo camCtr = new Bosch.VideoSDK.AxCameoLib.AxCameo();
        //    camCtr.Dock = DockStyle.Fill;

        //    pnl.Dock = System.Windows.Forms.DockStyle.Fill;
        //    pnl.Controls.Add(camCtr);    
        //    host.Child = pnl;
        //}


        //StatisticsWindow
    }
}