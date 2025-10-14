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

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for ExceptionalCheckOutView.xaml
    /// </summary>
    public partial class ExceptionalCheckOutView : BaseView
    {
        public new ExceptionalCheckOutViewModel ViewModel
        {
            get { return (ExceptionalCheckOutViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
        public ExceptionalCheckOutView()
        {
            InitializeComponent();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);
            this.FrontCamera.Camera = ViewModel.Section.FrontOutCamera;
            this.BackCamera.Camera = ViewModel.Section.BackOutCamera;
            ViewModel.Section.StartCameras(Core.Models.LaneDirection.Out);
            this.TextBoxReason.Focus();
        }

        private void Pass4Confirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(Pass4Confirm.Password) && Pass4Confirm.Password == "35422" && !string.IsNullOrEmpty(TextBoxReason.Text))
                btConfirm.IsEnabled = true;
            else
                btConfirm.IsEnabled = false;
        }

    }
}
