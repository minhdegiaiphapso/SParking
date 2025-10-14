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
    /// Interaction logic for FindImagesView.xaml
    /// </summary>
    public partial class FindImagesView : BaseView
    {
        public new FindImagesViewModel ViewModel
        {
            get { return (FindImagesViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        Dictionary<int, Grid> _maps;

        public FindImagesView()
        {
            InitializeComponent();
            _maps = new Dictionary<int, Grid>();
            LayoutView();
        }

        public override void Start(BaseViewModel viewModel)
        {
            base.Start(viewModel);
        }

        public void LayoutView()
        {
            for (int i = 0; i < 1; i++)
            {
                var colDefinition = new ColumnDefinition();
                colDefinition.Width = new GridLength(1, GridUnitType.Star);
                this.MainGrid.ColumnDefinitions.Add(colDefinition);

                Grid grid = new Grid();
                _maps.Add(i, grid);
                this.MainGrid.Children.Add(grid);
                grid.SetValue(Grid.ColumnProperty, i);
            }
        }

        public override bool InterceptViewRequest(BaseView requestedView)
        {
            int idx = 0;
            while (_maps[idx].Children.Count > 0 && idx < _maps.Count) idx++;

            BaseViewModel requestedViewModel = requestedView.ViewModel as BaseViewModel;
            requestedView.MainWindow = this.MainWindow;
            _maps[idx].Children.Add(requestedView);
            return true;
        }
    }
}
