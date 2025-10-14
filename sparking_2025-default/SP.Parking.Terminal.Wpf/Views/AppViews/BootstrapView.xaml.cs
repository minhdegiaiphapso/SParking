using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.ViewModels;
using SP.Parking.Terminal.Wpf.Devices;
using Green.Devices.Dal;
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

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for BootstrapView.xaml
    /// </summary>
    public partial class BootstrapView : BaseView
    {
        public new BootstrapViewModel ViewModel
        {
            get { return (BootstrapViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public BootstrapView()
        {
            InitializeComponent();

            IOptionsSettings _optionSettings = Mvx.Resolve<IOptionsSettings>();
            switch (_optionSettings.CameraType)
            {
                case CameraType.Hik:
                    Mvx.RegisterType<ICamera, HIKCamera>();
                    break;
                case CameraType.Vivotek:
                    Mvx.RegisterType<ICamera, VitaminCamera>();
                    break;
                case CameraType.Webcam:
                    Mvx.RegisterType<ICamera, Webcam>();
                    break;
                case CameraType.Bosch:
                    Mvx.RegisterType<ICamera, BoschCamera>();
                   
                    break;
            }
        }
    }
}
