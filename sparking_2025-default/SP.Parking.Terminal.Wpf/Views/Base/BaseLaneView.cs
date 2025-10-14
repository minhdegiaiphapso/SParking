using MahApps.Metro.Controls;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SP.Parking.Terminal.Wpf.UI;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace SP.Parking.Terminal.Wpf.Views
{
    public class BaseLaneView : BaseView
    {
        
        public new BaseLaneViewModel ViewModel
        {
            get { return (BaseLaneViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        bool _showChooseVehicleType = false;
        public bool ShowChooseVehicleType
        {
            get { return _showChooseVehicleType; }
            set
            {
                _showChooseVehicleType = value;
                if (_showChooseVehicleType)
                {
                    TypeHelper.GetVehicleTypes(types => {
                        this.Dispatcher.Invoke(() => {
                            ChooseVehicleTypeWindow window = new ChooseVehicleTypeWindow(types) { Container = this };
                            window.ChoseVehicleType += (sender, e) => {
                                ViewModel.ChooseVehicleType((e as ChooseVehicleTypeArg).VehicleType);
                            };
                            window.ShowDialog();     
                        });
                    });
                    _showChooseVehicleType = false;
                }
            }
        }

        public override void ViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<BaseLaneView, BaseLaneViewModel>();
            set.Bind(this).For(v => v.ShowChooseVehicleType).To(vm => vm.ShowChooseVehicleType);
            set.Apply();
        }

        MemoryStream _ms = null;
        BitmapImage _biImg = null;

        protected void ConvertNow(byte[] value, Image img)
        {
            try
            {
                if (value == null) return;

                _ms = new MemoryStream(value);
                _biImg = new BitmapImage();
                _biImg.BeginInit();
                _biImg.StreamSource = _ms;
                _biImg.EndInit();
     
                img.Source = _biImg;
                _biImg = null;
            }
            catch { return; }
            finally
            {
            }
        }
    }
}
