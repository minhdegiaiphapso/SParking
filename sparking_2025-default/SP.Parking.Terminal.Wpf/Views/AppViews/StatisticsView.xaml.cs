using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Media;
//using System.Windows.Data;
//using System.Windows.Input;
//using System.Windows.Media;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for StatisticsView.xaml
    /// </summary>
    public partial class StatisticsView : BaseView
    {
        private Statistics _statistics;
        public Statistics Statistics
        {
            get { return _statistics; }
            set
            {
                _statistics = value;
                Setup(_statistics);
            }
        }

        Dictionary<int, Point> _maps = new Dictionary<int, Point>();

        public new StatisticsViewModel ViewModel
        {
            get { return (StatisticsViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public StatisticsView()
        {
            InitializeComponent();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);
            BindData();
        }

        public void Setup(Statistics data)
        {
            if (data == null) return;
            int numberOfColumnItem =  typeof(StatisticsItem).GetProperties().Length;
            int numberCol = data.CardTypes.Count * numberOfColumnItem + 1;
            int numberRow = data.VehicleType.Count + 2;

            for (int i = 0; i < numberCol; i++)
            {
                var colDefinition = new ColumnDefinition();
                colDefinition.Width = new GridLength(1, GridUnitType.Star);
                this.MainGrid.ColumnDefinitions.Add(colDefinition);
            }

            for (int i = 0; i < numberRow; i++)
            {
                var rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(1, GridUnitType.Auto);
                rowDefinition.MinHeight = 40;
                this.MainGrid.RowDefinitions.Add(rowDefinition);
            }

            TextBlock tb = new TextBlock();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.FontSize = 16;
            tb.Foreground = Brushes.DarkGreen;
            tb.Text = "Loại xe";

            Border myBorder = new Border();
            myBorder.BorderBrush = Brushes.Gray;
            myBorder.BorderThickness = new Thickness(1);
            myBorder.SetValue(Grid.ColumnProperty, 0);
            myBorder.SetValue(Grid.RowProperty, 0);
            myBorder.SetValue(Grid.RowSpanProperty, 2);
            myBorder.Child = tb;

            this.MainGrid.Children.Add(myBorder);

            // setup cardtype column header
            for (int i = 1; i < numberCol; i++)
            {
                // setup cardtype header
                if (i % 3 == 1)
                {
                    CardType cardType = data.CardTypes[i / 3];
                    TextBlock tb1 = new TextBlock();
                    tb1.HorizontalAlignment = HorizontalAlignment.Center;
                    tb1.VerticalAlignment = VerticalAlignment.Center;
                    tb1.Text = cardType.Name;
                    tb1.Foreground = Brushes.DarkGreen;
                    tb1.FontSize = 16;
                    Border myBorder1 = new Border();
                    myBorder1.BorderBrush = Brushes.Gray;
                    myBorder1.BorderThickness = new Thickness(1);
                    myBorder1.SetValue(Grid.ColumnSpanProperty, (numberOfColumnItem));
                    myBorder1.SetValue(Grid.ColumnProperty, i);
                    myBorder1.SetValue(Grid.RowProperty, 0);
                    myBorder1.Child = tb1;

                    this.MainGrid.Children.Add(myBorder1);
                    int cardId = cardType.Id;
                    //int cardId = data.CardTypes.Where(c => c.Name.Equals(tb1.Text)).Select(c => c.Id).FirstOrDefault();
                    _maps.Add(cardId, new Point(i, 0));
                }

                // checkin/out/remain header
                int remainder = i % 3;

                TextBlock tb2 = new TextBlock();
                tb2.HorizontalAlignment = HorizontalAlignment.Center;
                tb2.VerticalAlignment = VerticalAlignment.Center;
                tb2.Foreground = Brushes.DarkGreen;
                tb2.FontSize = 16;

                Border myBorder2 = new Border();
                myBorder2.BorderBrush = Brushes.Gray;
                myBorder2.BorderThickness = new Thickness(1);
                myBorder2.SetValue(Grid.RowProperty, 1);
                myBorder2.SetValue(Grid.ColumnProperty, i);
                myBorder2.Child = tb2;
                switch (remainder)
                {
                    case 0:
                        tb2.Text = "Còn lại";
                        break;
                    case 1:
                        tb2.Text = "Xe vào";
                        break;
                    case 2:
                        tb2.Text = "Xe ra";
                        break;
                }

                this.MainGrid.Children.Add(myBorder2);
            }

            // setup row (vehicle type)
            for (int i = 2; i < numberRow; i++)
            {
                TextBlock tb1 = new TextBlock();
                tb1.HorizontalAlignment = HorizontalAlignment.Center;
                tb1.VerticalAlignment = VerticalAlignment.Center;
                tb1.FontSize = 16;
                tb1.Text = data.VehicleType[i - 2].Name;

                Border myBorder1 = new Border();
                myBorder1.BorderBrush = Brushes.Gray;
                myBorder1.BorderThickness = new Thickness(1);
                myBorder1.SetValue(Grid.ColumnProperty, 0);
                myBorder1.SetValue(Grid.RowProperty, i);
                myBorder1.Child = tb1;

                this.MainGrid.Children.Add(myBorder1);
                int vehicleId = data.VehicleType.Where(v => v.Name.Equals(tb1.Text)).Select(v => v.Id).FirstOrDefault();
                _maps.Add(vehicleId, new Point(0, i));
            }

            SetupContent(data);
        }

        public void SetupContent(Statistics data)
        {
            if (data == null) return;
            int numberOfColumnItem = typeof(StatisticsItem).GetProperties().Length;
            int numberCol = data.CardTypes.Count * numberOfColumnItem + 1;
            int numberRow = data.VehicleType.Count + 2;

            for (int j = 2; j < numberRow; j++)
            {
                for (int i = 1; i < numberCol; i++)
                {
                    TextBlock tb = new TextBlock();
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.FontSize = 16;
                    tb.Text = NhetDataVoNe(j, i, data).ToString();

                    Border myBorder = new Border();
                    myBorder.BorderBrush = Brushes.Gray;
                    myBorder.BorderThickness = new Thickness(1);
                    myBorder.SetValue(Grid.ColumnProperty, i);
                    myBorder.SetValue(Grid.RowProperty, j);
                    myBorder.Child = tb;

                    this.MainGrid.Children.Add(myBorder);
                }
            }
        }

        public int NhetDataVoNe(int row, int column, Statistics data)
        {
            int a = (column - 1) / 3;
            a = a * 3 + 1;

            int cardId = _maps.Where(p => p.Value.X == a).Select(p => p.Key).FirstOrDefault();
            int vehicleId = _maps.Where(p => p.Value.Y == row).Select(p => p.Key).FirstOrDefault();

            var item = data.Data[vehicleId.ToString()][cardId.ToString()];
            switch(column % 3)
            {
                case 0:
                    return item.Remain;
                case 1:
                    return item.CheckInNumber;
                case 2:
                    return item.CheckOutNumber;
            }

            return 0;
        }

        public override void BindData()
        {
            base.BindData();

            var set = this.CreateBindingSet<StatisticsView, StatisticsViewModel>();
            set.Bind(this).For(v => v.Statistics).To(vm => vm.Statistics);

            set.Apply();
        }
    }
}
