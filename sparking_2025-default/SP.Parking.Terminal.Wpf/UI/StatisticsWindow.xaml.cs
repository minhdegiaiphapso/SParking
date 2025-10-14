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
    /// Interaction logic for StatisticsWindow.xaml
    /// </summary>
    public partial class StatisticsWindow : MetroWindow
    {
        public StatisticsWindow()
        {
            InitializeComponent();

            Setup();

            this.KeyUp += StatisticsWindow_KeyUp;
        }

        void StatisticsWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void Setup()
        {
            var statisticsView = (StatisticsView)Mvx.Resolve<IMvxSimpleWpfViewLoader>().CreateView(MvxViewModelRequest<StatisticsViewModel>.GetDefaultRequest());
            //var vm = Mvx.IocConstruct<StatisticsViewModel>();
            //statisticsView.ViewModel = vm;
            statisticsView.ViewModel.PresentationObject = statisticsView;
            this.MainGrid.Children.Add(statisticsView);
            //statisticsView.Start(vm);
        }
    }
}
