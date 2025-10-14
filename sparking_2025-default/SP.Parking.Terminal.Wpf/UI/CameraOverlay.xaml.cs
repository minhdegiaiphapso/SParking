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

namespace SP.Parking.Terminal.Wpf.UI
{
	/// <summary>
	/// Interaction logic for CameraOverlay.xaml
	/// </summary>
	public partial class CameraOverlay : Window
	{
		private UserControl _container;
		public UserControl Container
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

        

		public object TextContent
		{
			get { return lbContent.Content; }
			set { lbContent.Content = value; }
		}

		public CameraOverlay()
		{
           
                InitializeComponent();

            //this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            //this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

            //this.Visibility = System.Windows.Visibility.Hidden;
            //try
            //{
            //    System.Windows.Application.Current.MainWindow.LayoutUpdated += (s, e) =>
            //    {
            //        if (Container == null)
            //            return;
            //        var window = Window.GetWindow(Container);
            //        if (window != null && window.IsVisible)
            //        {
            //            var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
            //            this.Left = pt.X;
            //            this.Top = pt.Y;
            //        }
            //    };
            //    System.Windows.Application.Current.MainWindow.LocationChanged += (s, e) =>
            //    {
            //        if (Container == null)
            //            return;
            //        var window = Window.GetWindow(Container);
            //        if (window != null && window.IsVisible)
            //        {
            //            var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
            //            this.Left = pt.X;
            //            this.Top = pt.Y;
            //        }
            //    };
            //}
            //catch
            //{ }

        }


        private void ContainerLayoutUpdated(object sender, EventArgs e)
		{
			if (Container == null)
				return;

            this.Width = Container.ActualWidth;
            this.Height = Container.ActualHeight;

			//WindowInteropHelper helper = new WindowInteropHelper(owner);
			//var handle = (new WindowInteropHelper(System.Windows.Application.Current.MainWindow)).Handle.ToInt32();
			var window = Window.GetWindow(Container);
			if (window != null && window.IsVisible)
			{
				this.Owner = window;
				if (this.Visibility == Visibility.Hidden)
					this.Show();
			}
			else
			{
				this.Owner = null;
				if (this.Visibility == Visibility.Visible)
					this.Hide();
			}
		}
		
	}
}
