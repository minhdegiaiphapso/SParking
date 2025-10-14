using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
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
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for LaneConfigurationView.xaml
    /// </summary>
    public partial class LaneConfigurationView : BaseView
    {
        public new LaneConfigurationViewModel ViewModel
        {
            get { return (LaneConfigurationViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
        public LaneConfigurationView()
        {
            InitializeComponent();
        }

        public void SetupLayout()
        {
            foreach(var item in ViewModel.LaneConfigViewModels)
            {
                var view = (SubLaneConfigurationView)Presenter.CreateView<SubLaneConfigurationViewModel>();
                view.ViewModel = item;
                this.MainGrid.Children.Add(view);
                view.SetValue(Grid.ColumnProperty, 1);
                view.SetValue(Grid.RowProperty, (int)view.ViewModel.Section.Id);
                
            }
        }

        private List< Section> _sections;
        public List<Section> Sections
        {
            get { return _sections; }
            set
            {
                if (_sections == value) return;
                _sections = value;
                SetupLayout();
            }
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            BindData();
        }

        public void SetupFooter()
        {
            Button saveBtn = new Button();
            saveBtn.Content = "Bắt đầu";
            saveBtn.HorizontalAlignment = HorizontalAlignment.Center;
            saveBtn.VerticalAlignment = VerticalAlignment.Center;
            saveBtn.Width = 152;
            saveBtn.Height = 60;
            saveBtn.FontSize = 24;
            saveBtn.SetResourceReference(Control.StyleProperty, "AccentedSquareButtonStyle");
            this.MainGrid.Children.Add(saveBtn);
            saveBtn.SetValue(Grid.RowProperty, 2);
            saveBtn.SetValue(Grid.ColumnProperty, 1);
            saveBtn.Click += (sender, e) => {
                ViewModel.SaveConfig();
            };
        }
        
        //public override bool InterceptViewRequest(BaseView requestedView)
        //{
        //    if(requestedView is SubLaneConfigurationView)
        //    {
        //        this.MainGrid.Children.Add(requestedView);
        //        requestedView.SetValue(Grid.ColumnProperty, 1);
        //        return true;
        //    }
        //    return false;
        //}

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<LaneConfigurationView, LaneConfigurationViewModel>();
            set.Bind(this).For(v => v.Sections).To(vm => vm.Sections);
            set.Apply();
        }
    }
}