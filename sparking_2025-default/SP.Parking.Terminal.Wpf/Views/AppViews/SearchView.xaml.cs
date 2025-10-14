using SP.Parking.Terminal.Core.Utilities;
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
using Xceed.Wpf.Toolkit;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    public class SearchTextBox : TextBox
    {
        public event KeyEventHandler SearchTextBoxKeyUp;
        public SearchTextBox()
            : base()
        {
            this.AddHandler(HookTextBox.KeyUpEvent, new RoutedEventHandler(SearchTextBox_KeyUp), true);
        }

        private void SearchTextBox_KeyUp(object sender, RoutedEventArgs e)
        {
            KeyEventArgs args = e as KeyEventArgs;
            switch (args.Key)
            {
                case Key.PageDown:
                case Key.PageUp:
                case Key.End:
                    e.Handled = true;
                    if (SearchTextBoxKeyUp != null)
                        SearchTextBoxKeyUp(sender, e as KeyEventArgs);
                    break;
            }
        }
    }

    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : BaseView
    {
        public new SearchViewModel ViewModel
        {
            get { return (SearchViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public SearchView()
        {
            InitializeComponent();

            ParkingSessionGrid.AddHandler(DataGrid.KeyUpEvent, new RoutedEventHandler(DataGrid_KeyUp), true);
           
            tbCardLabel.KeyUp += SearchTextBoxKeyUp;
            tbPlateNumber.KeyUp += SearchTextBoxKeyUp;
        }

        void SearchTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.KeyPressed(sender, e);
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            tbCardLabel.Focusable = true;
            tbCardLabel.Focus();
        }

        private void DataGrid_KeyUp(object sender, RoutedEventArgs e)
        {
            //Key key = (e as KeyEventArgs).Key;
            //if (key == Key.Delete)
            //    ViewModel.KeyPressed(sender, e as KeyEventArgs);
        }

        private void ParkingSessionGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var text = (e.EditingElement as TextBox).Text;
            ViewModel.EditEndingCommand.Execute(text);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectPageCommand.Execute(null);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb == null) return;

            ViewModel.IsCheckedCurrentUser = (bool)cb.IsChecked;
            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb == null) return;

            ViewModel.IsCheckedCurrentUser = (bool)cb.IsChecked;
        }
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ViewModel.SelectPageCommand.Execute(null);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnPreview.Visibility != Visibility.Visible)
            {
                pnPreview.Height = pnContainer.ActualHeight / 2;
                pnPreview.Width = pnContainer.ActualWidth / 1.5;
                Image img = sender as Image;
                viewImg.Source = img.Source;
                pnPreview.Visibility = Visibility.Visible;
            }
            else
                pnPreview.Visibility = Visibility.Hidden;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewImg.Source = null;
            pnPreview.Visibility = Visibility.Hidden;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Section.IsInExtra || ViewModel.Section.IsOutExtra)
            {
                extra11img.Visibility = extra11txt.Visibility = extra1img.Visibility = extra1txt.Visibility = extra22img.Visibility = extra22txt.Visibility = extra2img.Visibility = extra2txt.Visibility = Visibility.Visible;
            }
            else
            {
                extra11img.Visibility = extra11txt.Visibility = extra1img.Visibility = extra1txt.Visibility = extra22img.Visibility = extra22txt.Visibility = extra2img.Visibility = extra2txt.Visibility = Visibility.Hidden;

            }
        }

        private void extra1img_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void extra1img_LostFocus(object sender, RoutedEventArgs e)
        {

        }
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
           
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
           
        }

        private void ParkingSessionGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid grd = sender as DataGrid;
            //if (ViewModel.IsVoucher)
            //{
            //    grd.Columns[3].Visibility = Visibility.Visible;
            //    grd.Columns[4].Visibility = Visibility.Visible;
            //    grd.Columns[5].Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    grd.Columns[3].Visibility = Visibility.Hidden;
            //    grd.Columns[4].Visibility = Visibility.Hidden;
            //    grd.Columns[5].Visibility = Visibility.Hidden;
            //}
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewImg.Source = null;
            pnPreview.Visibility = Visibility.Hidden;
        }
    }
}
