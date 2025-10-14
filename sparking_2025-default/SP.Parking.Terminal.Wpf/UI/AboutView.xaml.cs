using Cirrious.CrossCore;
using MahApps.Metro.Controls;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using SP.Parking.Terminal.Core.Models;

namespace SP.Parking.Terminal.Wpf.UI
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : MetroWindow
    {
        private UIElement _container;
        public UIElement Container
        {
            get { return _container; }
            set
            {
                _container = value;
            }
        }

        IHostSettings _hostSettings;

        public AboutView()
        {
            InitializeComponent();

            this.Topmost = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            this.tblVersion.Text = "Version: " + version;
            Task.Factory.StartNew(() => {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    this.tblStorage.Text = "Log storage: " + (DirectorySize(new DirectoryInfo(GetLogPath()), true) / (1024 * 1024)).ToString("0.0 MB");
                });
            });

            _hostSettings = Mvx.Resolve<IHostSettings>();
            tblGuId.Text = _hostSettings.Terminal.TerminalId;

            this.KeyUp += AboutView_KeyUp;

            System.Windows.Application.Current.MainWindow.LayoutUpdated += (s, e) => {
                if (Container == null)
                    return;
                var window = Window.GetWindow(Container);
                if (window != null && window.IsVisible)
                {
                    var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                }
            };
            System.Windows.Application.Current.MainWindow.LocationChanged += (s, e) => {
                if (Container == null)
                    return;
                var window = Window.GetWindow(Container);
                if (window != null && window.IsVisible)
                {
                    var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                }
            };
        }

        private void DeleteCommand()
        {
            DeleteOldFiles(new DirectoryInfo(GetLogPath()));
        }

        private void DeleteOldFiles(DirectoryInfo dInfo)
        {
            var now = TimeMapInfo.Current.LocalTime;
            var result = dInfo.EnumerateFiles().Where(f => f.LastAccessTime < now - TimeSpan.FromDays(7));
            //var result = dInfo.EnumerateFiles().Where(f => f.LastAccessTime < DateTime.Now - TimeSpan.FromDays(7));
            foreach (var f in result)
            {
                f.Delete();
            }

            var dirs = dInfo.EnumerateDirectories();
            foreach (var dir in dirs)
            {
                DeleteOldFiles(dir);
            }

            if (!dInfo.Name.Equals("logs", StringComparison.CurrentCultureIgnoreCase))
                dInfo.Delete();
        }

        private double DirectorySize(DirectoryInfo dInfo, bool includeSubDir)
        {
            double totalSize = dInfo.EnumerateFiles().Sum(file => file.Length);

            if (includeSubDir)
            {
                totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir, true));
            }
            return totalSize;
        }

        void AboutView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            
            e.Handled = true;
        }

        private string GetLogPath()
        {
            string path = Path.Combine(_hostSettings.StoragePath, "logs");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        private void Hyperlink_RequestNavigate_1(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                string msg = string.Format("mailto:{0}", e.Uri.OriginalString);
                Process.Start(msg);
                e.Handled = true;
            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => {
                DeleteCommand();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    this.tblStorage.Text = "Log storage: " + (DirectorySize(new DirectoryInfo(GetLogPath()), true) / (1024 * 1024)).ToString("0.0 MB");
                });
            });
        }
    }
}
