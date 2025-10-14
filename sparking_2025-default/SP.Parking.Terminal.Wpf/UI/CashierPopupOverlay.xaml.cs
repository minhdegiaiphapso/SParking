using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Wpf.Views.AppViews;
using Green.Devices.Dal.CashierCounter;
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
    public partial class CashierPopupOverlay : Window, INotifyPropertyChanged
    {
        private CashierITC info;
        public CashierITC Info
        {
            get { return info; }
            set
            {
                if (info == value)
                    return;
                info = value;
              
                NotifyPropertyChanged("Info");
            }
        }
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
        private bool hidden = false;
        public bool Hidden
        {
            get { return hidden; }
            set
            {
                if (hidden == value)
                    return;
                hidden = value;
                if (hidden)
                    
                    this.Visibility = Visibility.Hidden;
                else
                    this.Visibility = Visibility.Visible;
                NotifyPropertyChanged("Hidden");
            }
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
        public CashierPopupOverlay()
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
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width);
                    this.Top = pt.Y + 35;
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
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width);
                    this.Top = pt.Y + 35;
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
            var CheckOutView = Container as CheckOutLaneView;
            var Sec = CheckOutView.ViewModel.Section;
            if (Sec != null && Sec.ComIctCashierEnanble && !string.IsNullOrEmpty(Sec.ComIctCashier))
                ItcCashierWrapper.GetItcCashier(Sec.ComIctCashier).CloseTransaction();
            Hidden = true;
            this.Visibility = Visibility.Hidden;   
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            var CheckOutView = Container as CheckOutLaneView;
            var Sec = CheckOutView.ViewModel.Section;
            if (Sec != null && Sec.ComIctCashierEnanble && !string.IsNullOrEmpty(Sec.ComIctCashier))
                ItcCashierWrapper.GetItcCashier(Sec.ComIctCashier).Reset();
        }

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            var CheckOutView = Container as CheckOutLaneView;
            var Sec = CheckOutView.ViewModel.Section;
            if (Sec != null && Sec.ComIctCashierEnanble && !string.IsNullOrEmpty(Sec.ComIctCashier))
                ItcCashierWrapper.GetItcCashier(Sec.ComIctCashier).Enable();
        }

        private void BtnDisable_Click(object sender, RoutedEventArgs e)
        {
            var CheckOutView = Container as CheckOutLaneView;
            var Sec = CheckOutView.ViewModel.Section;
            if (Sec != null && Sec.ComIctCashierEnanble && !string.IsNullOrEmpty(Sec.ComIctCashier))
                ItcCashierWrapper.GetItcCashier(Sec.ComIctCashier).Disable();
          
        }
    }
}
