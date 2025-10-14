using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for KeyConfigurationView.xaml
    /// </summary>
    public partial class KeyConfigurationView : BaseView
    {
        public new KeyConfigurationViewModel ViewModel
        {
            get { return (KeyConfigurationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        Dictionary<SectionPosition, Grid> _maps;

        List<Section> _sections;
        public List<Section> Sections
        {
            get { return _sections; }
            set
            {
                if (_sections == value)
                    return;

                _sections = value;

                if (_sections != null)
                    LayoutView();
            }
        }

        public KeyConfigurationView()
        {
            InitializeComponent();
            _maps = new Dictionary<SectionPosition, Grid>();
        }

        public void LayoutView()
        {
            foreach (var item in Sections)
            {
                var colDefinition = new ColumnDefinition();
                colDefinition.Width = new GridLength(1, GridUnitType.Star);
                this.MainGrid.ColumnDefinitions.Add(colDefinition);

                Grid grid = new Grid();
                _maps.Add(item.Id, grid);
                this.MainGrid.Children.Add(grid);
                grid.SetValue(Grid.ColumnProperty, (int)item.Id);
            }
        }

        public override void Start(BaseViewModel viewModel)
        {
            BindData();
            base.Start(viewModel);
        }

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<KeyConfigurationView, KeyConfigurationViewModel>();
            set.Bind(this).For(v => v.Sections).To(vm => vm.Sections);
            set.Apply();
        }

        public override bool InterceptViewRequest(BaseView requestedView)
        {
            BaseViewModel requestedViewModel = requestedView.ViewModel as BaseViewModel;
            requestedView.MainWindow = this.MainWindow;
            _maps[requestedViewModel.Section.Id].Children.Add(requestedView);
            return true;
        }
    }
}