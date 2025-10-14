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
using System.Windows.Forms;
using SP.Parking.Terminal.Core.ViewModels;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for ServerIPConfigView.xaml
    /// </summary>
    public partial class ServerIPConfigView : BaseView
    {
        private FolderBrowserDialog _browseFolderDialog;
        public new GeneralConfigViewModel ViewModel
        {
            get { return (GeneralConfigViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
        public ServerIPConfigView()
        {
            InitializeComponent();
            _browseFolderDialog = new FolderBrowserDialog();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            DialogResult rs = _browseFolderDialog.ShowDialog();
            if(rs == DialogResult.OK)
            {
                ViewModel.StoragePath = _browseFolderDialog.SelectedPath;
            }
        }
		private void btnBrowseAnpr_Click(object sender, RoutedEventArgs e)
		{
			DialogResult rs = _browseFolderDialog.ShowDialog();
			if (rs == DialogResult.OK)
			{
				ViewModel.StorageANPR = _browseFolderDialog.SelectedPath;
			}
		}
	}
}
