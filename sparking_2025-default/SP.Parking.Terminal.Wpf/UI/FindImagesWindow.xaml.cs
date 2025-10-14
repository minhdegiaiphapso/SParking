using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using MahApps.Metro.Controls;
using SP.Parking.Terminal.Core.ViewModels;
using SP.Parking.Terminal.Wpf.Views.AppViews;
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
using System.Windows.Shapes;

namespace SP.Parking.Terminal.Wpf.UI
{
    /// <summary>
    /// Interaction logic for FindImagesWindow.xaml
    /// </summary>
    public partial class FindImagesWindow : MetroWindow
    {
        public FindImagesWindow()
        {
            InitializeComponent();
            Setup();

            this.KeyUp += FindImagesWindow_KeyUp;
        }

        void FindImagesWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void Setup()
        {
            var FindImagesView = (FindImagesView)Mvx.Resolve<IMvxSimpleWpfViewLoader>().CreateView(MvxViewModelRequest<FindImagesViewModel>.GetDefaultRequest());
            var vm = Mvx.IocConstruct<FindImagesViewModel>();
            FindImagesView.ViewModel = vm;
            FindImagesView.ViewModel.PresentationObject = FindImagesView;
            this.MainGrid.Children.Add(FindImagesView);
            FindImagesView.Start(vm);
        }
    }
}
