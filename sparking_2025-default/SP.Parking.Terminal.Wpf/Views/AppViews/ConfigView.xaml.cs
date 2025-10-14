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
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : BaseView
    {
        public new ConfigViewModel ViewModel
        {
            get { return (ConfigViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        private int _currentIndex = 0;

        private string _itemname = string.Empty;

        public LaneConfigurationView LaneConfigurationView { get; private set; }
        public ServerIPConfigView ServerConfigurationView { get; private set; }
        public OptionsConfigurationView OptionsConfigurationView { get; private set; }
        public InputCardView InputCardView { get; private set; }
        public KeyConfigurationView KeyConfigurationView { get; private set; }
        public FindImagesView FindImagesView { get; private set; }

        public ConfigView()
        {
            InitializeComponent();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            UpdateTabDisplay();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentIndex == this.MainTabControl.SelectedIndex) return;

            _currentIndex = this.MainTabControl.SelectedIndex;
            
            UpdateTabDisplay();
        }

        private void UpdateTabDisplay()
        {
            switch (_currentIndex)
            {
                case 0:
                    {
                        if (ServerConfigurationView != null) return;
                        ServerConfigurationView = (ServerIPConfigView)Presenter.CreateView<GeneralConfigViewModel>();
                        ServerConfigurationView.ViewModel = ViewModel.GeneralConfigurationViewModel;
                        this.ServerConfigGrid.Children.Add(ServerConfigurationView);
                        ServerConfigurationView.Start(ViewModel.GeneralConfigurationViewModel);

                        break;
                    }
                case 1:
                    {
                        if (LaneConfigurationView != null) return;

                        LaneConfigurationView = (LaneConfigurationView)Presenter.CreateView<LaneConfigurationViewModel>();
                        LaneConfigurationView.ViewModel = ViewModel.LaneConfigurationViewModel;
                        this.LaneConfigGrid.Children.Add(LaneConfigurationView);
                        LaneConfigurationView.Start(ViewModel.LaneConfigurationViewModel);

                        break;
                    }
                case 3:
                    {
                        if (OptionsConfigurationView != null) return;
                        OptionsConfigurationView = (OptionsConfigurationView)Presenter.CreateView<OptionsConfigurationViewModel>();
                        OptionsConfigurationView.ViewModel = ViewModel.OptionsConfigurationViewModel;
                        OptionsConfigurationView.MainWindow = this.MainWindow;
                        this.OptionsConfigGrid.Children.Add(OptionsConfigurationView);
                        OptionsConfigurationView.Start(ViewModel.OptionsConfigurationViewModel);

                        break;
                    }
                case 4:
                    {
                        if (InputCardView != null) return;

                        InputCardView = (InputCardView)Presenter.CreateView<InputCardViewModel>();
                        InputCardView.ViewModel = ViewModel.InputCardViewModel;
                        this.InputCardGrid.Children.Add(InputCardView);
                        InputCardView.Start(ViewModel.InputCardViewModel);

                        break;
                    }
                case 2:
                    {
                        if (KeyConfigurationView != null) return;

                        KeyConfigurationView = (KeyConfigurationView)Presenter.CreateView<KeyConfigurationViewModel>();
                        KeyConfigurationView.ViewModel = ViewModel.KeyConfigurationViewModel;
                        KeyConfigurationView.ViewModel.PresentationObject = KeyConfigurationView;
                        this.KeyConfigurationGrid.Children.Add(KeyConfigurationView);
                        KeyConfigurationView.Start(ViewModel.KeyConfigurationViewModel);

                        break;
                    }
                //case 5:
                //    {
                //        if (FindImagesView != null) return;

                //        FindImagesView = (FindImagesView)Presenter.CreateView<FindImagesViewModel>();
                //        FindImagesView.ViewModel = ViewModel.FindImagesViewModel;
                //        FindImagesView.ViewModel.PresentationObject = FindImagesView;
                //        this.FindCardGrid.Children.Add(FindImagesView);
                //        FindImagesView.Start(ViewModel.FindImagesViewModel);
                //        break;
                //    }
            }
        }
    }
}
