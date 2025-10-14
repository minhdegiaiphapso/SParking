using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    public class HookTextBox : TextBox
    {
        public HookTextBox()
            : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            this.AddHandler(HookTextBox.KeyDownEvent, new RoutedEventHandler(HookTextBox_KeyDown), true);
        }

        public void HookTextBox_KeyDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.Text = KeyUtil.ConvertToString(e as KeyEventArgs);
        }
    }

    /// <summary>
    /// Interaction logic for LaneKeyConfigurationView.xaml
    /// </summary>
    public partial class LaneKeyConfigurationView : BaseView
    {
        public new LaneKeyConfigurationViewModel ViewModel
        {
            get { return (LaneKeyConfigurationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public LaneKeyConfigurationView()
        {
            InitializeComponent();
        }

        private void HookTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
