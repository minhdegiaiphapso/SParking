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
using SP.Parking.Terminal.Core.Services;
using Cirrious.CrossCore;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for LaneContainerView.xaml
    /// </summary>
    public partial class LaneContainerView : BaseView
    {
        IUserPreferenceService _preferenceService;

        Dictionary<SectionPosition, Grid> _maps;

        Dictionary<DisplayedPosition, Grid> _ohYeahMaps;

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

        private void LayoutView()
        {
			_ohYeahMaps.Clear();
			this.ContentGrid.Children.Clear();
			this.ContentGrid.RowDefinitions.Clear();
			this.ContentGrid.ColumnDefinitions.Clear();
			if (_preferenceService.HostSettings.ActualSections == 2)
			{
				LayoutFour();
			}
            else if (_preferenceService.HostSettings.ActualSections == 1)
            {
                LayoutThree();
            }
			else
			{
				LayoutNomal();
			}
        }
		private void LayoutNomal()
		{
			Sections[0].ShouldBeDisplayed = true;
			Sections[1].ShouldBeDisplayed = true;
			Sections[2].ShouldBeDisplayed = false;
			Sections[3].ShouldBeDisplayed = false;
			for (int i = 0; i < _preferenceService.SystemSettings.NumberOfDisplayedLane; i++)
			{
				Section item = Sections[i];
				var colDefinition = new ColumnDefinition();
				colDefinition.Width = new GridLength(1, GridUnitType.Star);
				this.ContentGrid.ColumnDefinitions.Add(colDefinition);
				Grid grid = new Grid();
				_ohYeahMaps.Add(item.DisplayedPosition, grid);
				this.ContentGrid.Children.Add(grid);
				grid.SetValue(Grid.ColumnProperty, (int)item.Id);
			}
		}
		private void LayoutThree()
		{
			Sections[0].ShouldBeDisplayed = true;
			Sections[1].ShouldBeDisplayed = true;
			Sections[2].ShouldBeDisplayed = true;
			Sections[3].ShouldBeDisplayed = false;
			var lcolDefinition = new ColumnDefinition();
			lcolDefinition.Width = new GridLength(1, GridUnitType.Star);
			var mcolDefinition = new ColumnDefinition();
			mcolDefinition.Width = new GridLength(1, GridUnitType.Star);
			var rcolDefinition = new ColumnDefinition();
			rcolDefinition.Width = new GridLength(1, GridUnitType.Star);

			this.ContentGrid.ColumnDefinitions.Add(lcolDefinition);
			Section itemLeft = Sections[0];
			itemLeft.DisplayedPosition = DisplayedPosition.Left;
			Grid Leftgrid = new Grid();
			_ohYeahMaps.Add(itemLeft.DisplayedPosition, Leftgrid);

			this.ContentGrid.Children.Add(Leftgrid);
			Leftgrid.SetValue(Grid.ColumnProperty, 0);

			this.ContentGrid.ColumnDefinitions.Add(mcolDefinition);
			Section itemMiddle = Sections[1];
			itemMiddle.DisplayedPosition = DisplayedPosition.Middle;
			Grid Middlegrid = new Grid();
			_ohYeahMaps.Add(itemMiddle.DisplayedPosition, Middlegrid);

			this.ContentGrid.Children.Add(Middlegrid);
			Middlegrid.SetValue(Grid.ColumnProperty, 1);

			this.ContentGrid.ColumnDefinitions.Add(rcolDefinition);
			Section itemRight = Sections[2];
			itemRight.DisplayedPosition = DisplayedPosition.Right;
			Grid Rightgrid = new Grid();
			_ohYeahMaps.Add(itemRight.DisplayedPosition, Rightgrid);

			this.ContentGrid.Children.Add(Rightgrid);
			Rightgrid.SetValue(Grid.ColumnProperty, 2);
		}
        private void LayoutFour()
        {
            // Active all four lane
            Sections[0].ShouldBeDisplayed = true;
            Sections[1].ShouldBeDisplayed = true;
            Sections[2].ShouldBeDisplayed = true;
            Sections[3].ShouldBeDisplayed = true;

            //Row Grid
            var trowDefinition = new RowDefinition();
            trowDefinition.Height = new GridLength(1, GridUnitType.Star);
            var browDefinition = new RowDefinition();
            browDefinition.Height = new GridLength(1, GridUnitType.Star);

            //Col Grid
            var lcolDefinition = new ColumnDefinition();
            lcolDefinition.Width = new GridLength(1, GridUnitType.Star);
            var rcolDefinition = new ColumnDefinition();
            rcolDefinition.Width = new GridLength(1, GridUnitType.Star);

            //Thiết lập Top Left
            this.ContentGrid.ColumnDefinitions.Add(lcolDefinition);
            this.ContentGrid.RowDefinitions.Add(trowDefinition);
            Section itemTopLeft = Sections[0];
            itemTopLeft.DisplayedPosition = DisplayedPosition.TopLeft;
            Grid LeftTopgrid = new Grid();
            _ohYeahMaps.Add(itemTopLeft.DisplayedPosition, LeftTopgrid);
            this.ContentGrid.Children.Add(LeftTopgrid);
            LeftTopgrid.SetValue(Grid.ColumnProperty, 0);
            LeftTopgrid.SetValue(Grid.RowProperty, 0);


            //THiết lập Top Right
            this.ContentGrid.ColumnDefinitions.Add(rcolDefinition);
			//this.ContentGrid.RowDefinitions.Add(trowDefinition);
			Section itemTopRight = Sections[1];
            itemTopRight.DisplayedPosition = DisplayedPosition.TopRight;
            Grid TopRightgrid = new Grid();
            _ohYeahMaps.Add(itemTopRight.DisplayedPosition, TopRightgrid);
            this.ContentGrid.Children.Add(TopRightgrid);
            TopRightgrid.SetValue(Grid.ColumnProperty, 1);
            TopRightgrid.SetValue(Grid.RowProperty, 0);

			//Thiết lập Bottom Left
			//this.ContentGrid.ColumnDefinitions.Add(lcolDefinition);
			this.ContentGrid.RowDefinitions.Add(browDefinition);
			Section itemBottomLeft = Sections[2];
            itemBottomLeft.DisplayedPosition = DisplayedPosition.BottomLeft;
            Grid BottomLeftgrid = new Grid();
            _ohYeahMaps.Add(itemBottomLeft.DisplayedPosition, BottomLeftgrid);
            this.ContentGrid.Children.Add(BottomLeftgrid);
            BottomLeftgrid.SetValue(Grid.ColumnProperty, 0);
            BottomLeftgrid.SetValue(Grid.RowProperty, 1);

			//Thiết lập Bottom Right
			//this.ContentGrid.ColumnDefinitions.Add(rcolDefinition);
			//this.ContentGrid.RowDefinitions.Add(browDefinition);
			Section itemBottomRight = Sections[3];
            itemBottomRight.DisplayedPosition = DisplayedPosition.BottomRight;
            Grid itemBottomRightGrid = new Grid();
            _ohYeahMaps.Add(itemBottomRight.DisplayedPosition, itemBottomRightGrid);
            this.ContentGrid.Children.Add(itemBottomRightGrid);
            itemBottomRightGrid.SetValue(Grid.ColumnProperty, 1);
            itemBottomRightGrid.SetValue(Grid.RowProperty, 1);
        }
        public new LaneContainerViewModel ViewModel
        {
            get { return (LaneContainerViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
        public LaneContainerView()
        {
            InitializeComponent();

            _maps = new Dictionary<SectionPosition, Grid>();

            _ohYeahMaps = new Dictionary<DisplayedPosition, Grid>();

            _preferenceService = Mvx.Resolve<IUserPreferenceService>();

            //BindData();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);
            //ViewModel.GetUser();
            //this.KeyUp += LaneContainerView_KeyUp;
        }

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<LaneContainerView, LaneContainerViewModel>();
            set.Bind(this).For(v => v.Sections).To(vm => vm.Sections);
            set.Apply();
        }

        public override bool InterceptViewRequest(BaseView requestedView)
        {
            BaseViewModel requestedViewModel = requestedView.ViewModel as BaseViewModel;

            if (!(requestedView.ViewModel is LoginViewModel))
            {
                requestedView.MainWindow = this.MainWindow;
                //_maps[requestedViewModel.Section.Id].Children.Add(requestedView);
            }

            if (requestedViewModel.Section.ShouldBeDisplayed)
                _ohYeahMaps[requestedViewModel.Section.DisplayedPosition].Children.Add(requestedView);
            return true;
        }

        public override bool InterceptCloseViewRequest(BaseViewModel childVM)
        {
            if (childVM is BaseLaneViewModel || 
                childVM is SearchViewModel || 
                childVM is LoginViewModel ||
                childVM is ExceptionalCheckOutViewModel ||
                childVM is EndingShiftInformationViewModel)
            {
                BaseViewModel vm = childVM as BaseViewModel;
                //Grid grid = _maps[vm.Section.Id];
                Grid grid = _ohYeahMaps[vm.Section.DisplayedPosition];
                grid.Children.RemoveAt(grid.Children.Count - 1);
                return true;
            }
            
            return false;
        }

        //void LaneContainerView_KeyUp(object sender, KeyEventArgs e)
        //{
        //    //this.ViewModel.KeyPressed(sender, e);
        //    Console.WriteLine(e.Key);
        //}
    }
}