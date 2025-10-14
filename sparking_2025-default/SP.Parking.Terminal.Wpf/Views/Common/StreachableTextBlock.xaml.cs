using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SP.Parking.Terminal.Wpf.Views.Common
{
    /// <summary>
    /// Interaction logic for StreachableTextBlock.xaml
    /// </summary>
    public partial class StreachableTextBlock
    {
		public static readonly DependencyProperty TBContentProperty = StreachableTextBox.TBContentProperty.AddOwner(typeof(StreachableTextBlock)); // DependencyProperty.Register("TBContent", typeof(string), typeof(StreachableTextBlock));        
        public string TBContent
        {
            get { return (string)GetValue(TBContentProperty); }
            set { SetValue(TBContentProperty, value); }
        }
        public StreachableTextBlock()
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
        }

        #region ShouldFocuse
		public static readonly DependencyProperty ShouldFocuseProperty = StreachableTextBox.ShouldFocuseProperty.AddOwner(typeof(StreachableTextBlock));  // DependencyProperty.RegisterAttached("ShouldFocuse", typeof(bool), typeof(StreachableTextBox), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        public bool ShouldFocuse
        {
            get { return (bool)GetValue(ShouldFocuseProperty); }
            set { SetValue(ShouldFocuseProperty, value); }
        }

		public static readonly DependencyProperty TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner(typeof(StreachableTextBlock));

		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentProperty); }
			set { SetValue(TextAlignmentProperty, value); }
		}

        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StreachableTextBox)
            {
                if ((bool)e.NewValue)
                {
                    TextBox tb = (d as StreachableTextBox).TextBoxChild;
                    tb.Focus();
                    tb.SelectAll();
                }
            }
        }
        #endregion

    }
}
