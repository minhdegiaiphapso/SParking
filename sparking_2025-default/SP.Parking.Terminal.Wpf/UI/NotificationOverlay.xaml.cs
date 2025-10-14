using Cirrious.MvvmCross.ViewModels;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace SP.Parking.Terminal.Wpf.UI
{
	/// <summary>
	/// Interaction logic for CameraOverlay.xaml
	/// </summary>
	public partial class NotificationOverlay : Window, INotifyPropertyChanged
	{
		private DispatcherTimer _hideTimer;
		private UIElement _container;
		public UIElement Container
		{
			get { return _container; }
			set {
				if (_container != null )
				{
					_container.LayoutUpdated -= ContainerLayoutUpdated;
					
				}
				_container = value;
				_container.LayoutUpdated += ContainerLayoutUpdated;
			}
		}
		private Notices _notices;
		public Notices Notices {
			get { return _notices; }
			set
			{
                if (value == null || value.Count <= 0)
                {
                    this.Hide();
                    _hideTimer.Stop();
                    this.Hidden = true;
                }
				if (_notices == value)
					return;
				_notices = value;
				NotifyPropertyChanged();
			}
		}
		public bool Hidden { get; set; }
		public NotificationOverlay()
		{
			InitializeComponent();
			this.Visibility = System.Windows.Visibility.Hidden;
			this.Hidden = true;
			System.Windows.Application.Current.MainWindow.LayoutUpdated += (s, e) =>
			{
				if (Container == null)
					return;
				var window = Window.GetWindow(Container);
				if (window != null && window.IsVisible)
				{
					var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    var left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
					//var left  = pt.X + (Container.RenderSize.Width - this.RenderSize.Width)/2;
					//left = left - Container.RenderSize.Width / 2;
					//if (left < 0)
					//   left = (Container.RenderSize.Width-this.Width)/2;
					//this.Left = left;

					this.Top = (pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2);
					
                    //this.Width = Container.RenderSize.Width - 40;

                }
			};
			System.Windows.Application.Current.MainWindow.LocationChanged += (s, e) =>
			{
				if (Container == null)
					return;
				var window = Window.GetWindow(Container);
				if (window != null && window.IsVisible)
				{
					var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    this.Left= pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    //var left  = pt.X + (Container.RenderSize.Width - this.RenderSize.Width)/2;
                    //left = left - Container.RenderSize.Width / 2;
                    //if (left < 0)
                    //   left = (Container.RenderSize.Width-this.Width)/2;
                    //this.Left = left;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                    //this.Width = Container.RenderSize.Width - 40;
                }
			};
			_hideTimer = new System.Windows.Threading.DispatcherTimer();
			_hideTimer.Tick += new EventHandler(dispatcherTimer_Tick);
		}

		public void ShowNotification(int timeToHideMs)
		{
			if (Notices.Count == 0)
				return;

			if (this.Visibility == Visibility.Hidden)
				this.Show();

			if (timeToHideMs > 0)
			{
				_hideTimer.Interval = TimeSpan.FromMilliseconds(timeToHideMs);
				_hideTimer.Stop();
				_hideTimer.Start();
			}

			this.Hidden = false;
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (this.Visibility == Visibility.Visible)
				this.Hide();
			_hideTimer.Stop();
            _notices = null;
			this.Hidden = true;
		}


		private void ContainerLayoutUpdated(object sender, EventArgs e)
		{
			if (Container == null)
				return;

			//WindowInteropHelper helper = new WindowInteropHelper(owner);
			//var handle = (new WindowInteropHelper(System.Windows.Application.Current.MainWindow)).Handle.ToInt32();
			var window = Window.GetWindow(Container);
			if (window != null && window.IsVisible)
			{
				this.Owner = window;
				if (this.Visibility == Visibility.Hidden && Hidden == false)
					this.Show();
			}
			else
			{
				this.Owner = null;
				if (this.Visibility == Visibility.Visible)
					this.Hide();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

	}
}
