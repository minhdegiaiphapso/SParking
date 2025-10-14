using SP.Parking.Terminal.Wpf.Views.Common;
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

namespace SP.Parking.Terminal.Wpf.UI
{
    /// <summary>
    /// Interaction logic for VehicleInfoUI.xaml
    /// </summary>
    public partial class VehicleInfoUI : UserControl
    {
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(string), typeof(VehicleInfoUI), new FrameworkPropertyMetadata(string.Empty, OnValueChanged));
        public string Number
        {
            get { return (string)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value); }
        }

        public static readonly DependencyProperty BrandProperty = DependencyProperty.Register("Brand", typeof(string), typeof(VehicleInfoUI), new FrameworkPropertyMetadata(string.Empty, OnValueChanged1));
        public string Brand
        {
            get { return (string)GetValue(BrandProperty); }
            set { SetValue(BrandProperty, value); }
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VehicleInfoUI textblock = sender as VehicleInfoUI;
            textblock.Number = e.NewValue.ToString();
        }

        private static void OnValueChanged1(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VehicleInfoUI textblock = sender as VehicleInfoUI;
            textblock.Brand = e.NewValue.ToString();
        }

        public VehicleInfoUI()
        {
            InitializeComponent();
        }
    }
}
