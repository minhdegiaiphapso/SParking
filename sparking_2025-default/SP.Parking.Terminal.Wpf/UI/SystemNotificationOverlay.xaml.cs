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
	public partial class SystemNotificationOverlay : Window, INotifyPropertyChanged
	{
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


		public string Text
		{
			get { return textContent.Text; }
			set { textContent.Text = value; }
		}

		public bool Hidden { get; set; }

		public SystemNotificationOverlay()
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
					this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width)/2;
					this.Top = pt.Y + 35;
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
					this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
					this.Top = pt.Y + 35;
				}
			};

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
