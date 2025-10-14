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
    /// Interaction logic for VehicleTypeUI.xaml
    /// </summary>
    public partial class VehicleTypeUI : UserControl
    {
        int _number;
        string _r;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
                NumberTB.Text = _number.ToString();
            }
        }

        string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set
            {
                _typeName = value;
                vehicle_name.Text = _typeName;
                vehicle_icon.Source = new BitmapImage(new Uri(@typeIcon(_typeName), UriKind.Relative));
            }
        }

        private string typeIcon(string _typeName)
        {
            _r = "/images/moto-icon.png";

            switch (_typeName.ToUpper().Trim())
            {
                case "XE ĐẠP":
                    {
                        _r = "/images/bike-icon.png";
                        break;
                    }
                case "XE ĐẠP ĐIỆN":
                    {
                        _r = "/images/electricbike-icon.png";
                        break;
                    }
                case "XE Ô-TÔ (DƯỚI 10 CHỖ)":
                case "XE Ô-TÔ (TRÊN 10 CHỖ)":
                case "XE TAXI":
                case "Ô TÔ":
                    {
                        _r = "/images/car-icon.png";
                        break;
                    }
                case "XE GẮN MÁY":
                case "XE MÁY":
                case "XE MIEN PHI":
                case "XE GA, XE GẮN MÁY TRÊN 175CM3":
                    {
                        _r = "/images/moto-icon.png";
                        break;
                    }
            }

            return _r;
        }

        public VehicleTypeUI()
        {
            InitializeComponent();
        }

    }
}
