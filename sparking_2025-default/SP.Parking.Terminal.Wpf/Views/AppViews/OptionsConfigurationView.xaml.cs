using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
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
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.CrossCore;
using Green.Devices.Dal;
using SP.Parking.Terminal.Wpf.Devices;
using SP.Parking.Terminal.Core.Services;
using MahApps.Metro;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for OptionsConfigurationView.xaml
    /// </summary>
    public partial class OptionsConfigurationView : BaseView
    {
        string _parkingName;
        public string ParkingName
        {
            get { return _parkingName; }
            set
            {
                if (_parkingName == value) return;
                _parkingName = value;
                this.MainWindow.Title = ParkingName;
            }
        }

        public new OptionsConfigurationViewModel ViewModel
        {
            get { return (OptionsConfigurationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public OptionsConfigurationView()
        {
            InitializeComponent();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            BindData();
        }

        public override void BindData()
        {
            base.BindData();

            //var set = this.CreateBindingSet<OptionsConfigurationView, OptionsConfigurationViewModel>();
            //set.Bind(this).For(v => v.ParkingName).To(vm => vm.ParkingName);
            //set.Apply();
        }

        private void CameraTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CameraType type = (CameraType)e.AddedItems[0];
            switch (type)
            {
                case CameraType.Hik:
                    Mvx.RegisterType<ICamera, HIKCamera>();
                    ViewModel.CameraTypes = null;
                    break;
                case CameraType.Vivotek:
                    Mvx.RegisterType<ICamera, VitaminCamera>();
                    ViewModel.CameraTypes = null;
                    break;
                case CameraType.Webcam:
                    Mvx.RegisterType<ICamera, Webcam>();
                    break;
                case CameraType.Bosch:
                    Mvx.RegisterType<ICamera, BoschCamera>();
                   
                    ViewModel.CameraTypes = null;
                    break;
            }
        }

        
    }
}