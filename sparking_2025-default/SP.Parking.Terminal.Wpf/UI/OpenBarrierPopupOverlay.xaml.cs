using SP.Parking.Terminal.Wpf.Views.AppViews;
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

namespace SP.Parking.Terminal.Wpf.UI
{
    /// <summary>
    /// Interaction logic for OpenBarrierPopupOverlay.xaml
    /// </summary>
    public partial class OpenBarrierPopupOverlay : Window, INotifyPropertyChanged
    {
        private UIElement _container;
        public UIElement Container
        {
            get { return _container; }
            set
            {
                if (_container != null)
                {
                    _container.LayoutUpdated -= ContainerLayoutUpdated;

                }
                _container = value;
                _container.LayoutUpdated += ContainerLayoutUpdated;
            }
        }
        public bool Hidden { get; set; }
       
        public bool IsCheckIn { get; set; }
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
        public OpenBarrierPopupOverlay()
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
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2; 
                    this.Width = Container.RenderSize.Width / 2 - 20;
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
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width)/2;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                    this.Width = Container.RenderSize.Width / 2 - 20;
                }
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)//Bỏ qua
        {
            Hidden = true;
            this.Visibility = Visibility.Hidden;
          
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//Đồng ý mở Barrier
        {
            if (!string.IsNullOrEmpty(TextBoxReason.Text) && !string.IsNullOrEmpty(Pass4Confirm.Password) && Pass4Confirm.Password== "35422")
            {
                if (IsCheckIn)
                {
                    var CheckInView = Container as CheckInLaneView;
                    CheckInView.ViewModel.ForcedBarier(TextBoxReason.Text);
                }
                else
                {
                    var CheckOutView = Container as CheckOutLaneView;
                    CheckOutView.ViewModel.ForcedBarier(TextBoxReason.Text);
                }
                Hidden = true;
                this.Visibility = Visibility.Hidden;
               
            }
           
           
        }
    }
}
