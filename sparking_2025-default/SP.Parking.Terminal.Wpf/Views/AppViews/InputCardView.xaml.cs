using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.IO;
using Cirrious.MvvmCross.ViewModels;

namespace SP.Parking.Terminal.Wpf.Views.AppViews
{
    /// <summary>
    /// Interaction logic for InputCardView.xaml
    /// </summary>
    public partial class InputCardView : BaseView
    {
        public new InputCardViewModel ViewModel
        {
            get { return (InputCardViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        private List<int> _brushedRowIndexes = new List<int>();

        public InputCardView()
        {
            InitializeComponent();
        }

        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

            // Scroll to lasted item
            ViewModel.CompletedReadingCard += OnCompletedReadingCard;
            ViewModel.DetectedDuplicatedCards += OnDetectedDuplicatedCards;
            ViewModel.SaveCompleted += OnSaveCompleted;
        }

        public void OnCompletedReadingCard(object sender, EventArgs e)
        {
            TapCardEventArgs a = e as TapCardEventArgs;
            var cards = a.Cards;
            HighLight(cards);
        }

        public void OnDetectedDuplicatedCards(object sender, EventArgs e)
        {
            TapCardEventArgs a = e as TapCardEventArgs;
            var cards = a.Cards;
            HighLight(cards);
        }

        private void HighLight(List<CardHolder> cards)
        {
            if (cards != null)
            {
                PaintRow(_brushedRowIndexes, Brushes.Transparent);
                _brushedRowIndexes = GetIndexOfItems(cards);
                PaintRow(_brushedRowIndexes, Brushes.Red);
                ScrollToRow(_brushedRowIndexes);
            }
            else
            {
                for (int i = 0; i < _brushedRowIndexes.Count; i++)
                    _brushedRowIndexes[i] += 1;
                
                PaintRow(_brushedRowIndexes, Brushes.Transparent);
                CardGrid.ScrollIntoView(CardGrid.Items[0]);
            }
        }

        public void OnSaveCompleted(object sender, EventArgs e)
        {
            SaveEventArgs args = e as SaveEventArgs;
            SaveButton.IsEnabled = args.IsCompleted;
        }

        private void PaintRow(List<int> indexes, SolidColorBrush color)
        {
            if (indexes.Count > 0)
            {
                foreach (var idx in indexes)
                {
                    DataGridRow row = (DataGridRow)CardGrid.ItemContainerGenerator.ContainerFromIndex(idx);
                    if (row != null)
                        row.BorderBrush = color;
                }
            }
        }

        private void ScrollToRow(List<int> indexes)
        {
            if (_brushedRowIndexes.Count > 0)
                CardGrid.ScrollIntoView(CardGrid.Items[_brushedRowIndexes[0]]);
        }

        private List<int> GetIndexOfItems(List<CardHolder> cards)
        {
            List<int> indexes = new List<int>();
            foreach (var card in cards)
            {
                var idx = this.CardGrid.Items.IndexOf(card);
                if (idx > -1)
                    indexes.Add(idx);
            }

            return indexes;
        }

        private void SaveCardsLocally()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "cards";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";

            Nullable<bool> rs = dlg.ShowDialog();

            if (rs == true)
            {
                string filename = dlg.FileName;

                ViewModel.DownloadAllCards((result, ex) => {
                    File.WriteAllText(filename, result);
                });
            }
        }

        private void Downloadbutton_Click(object sender, RoutedEventArgs e)
        {
            SaveCardsLocally();
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
