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
    /// Interaction logic for GetCheckedInformationView.xaml
    /// </summary>
    public partial class GetCheckedInformationView : BaseView
    {
        public new GetCheckedInformationViewModel ViewModel
        {
            get { return (GetCheckedInformationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public GetCheckedInformationView()
        {
            InitializeComponent();
            tbCardLabel.Focus();
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.KeyUpEvent(sender, e);
        }
    }
}
