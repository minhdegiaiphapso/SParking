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

namespace SP.Parking.Terminal.Wpf.Views.Common
{
    /// <summary>
    /// Interaction logic for StreachableTextBox.xaml
    /// </summary>
    public partial class StreachableTextBox
    {
        #region Constructor
        public StreachableTextBox()
        {
            InitializeComponent();
        } 
        #endregion

        //#region KeysPressed
        //public static readonly DependencyProperty KeysPressedProperty = DependencyProperty.RegisterAttached(
        //    "KeysPressed", typeof(DependencyProperty), typeof(StreachableTextBox), new PropertyMetadata(null, OnKeysPressedProperty));

        //public static void SetKeysPressed(DependencyObject dp, DependencyProperty value)
        //{
        //    dp.SetValue(KeysPressedProperty, value);
        //}

        //public static DependencyProperty GetKeysPressed(DependencyObject dp)
        //{
        //    return (DependencyProperty)dp.GetValue(KeysPressedProperty);
        //}

        //private static void OnKeysPressedProperty(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        //{
        //    Console.WriteLine("sdfdf");
        //} 
        //#endregion

        #region TBContent
        public static readonly DependencyProperty TBContentProperty = DependencyProperty.Register("TBContent", typeof(string), typeof(StreachableTextBox), new FrameworkPropertyMetadata(string.Empty, OnTBContentChanged));
        public string TBContent
        {
            get { return (string)GetValue(TBContentProperty); }
            set { SetValue(TBContentProperty, value); }
        }
        private static void OnTBContentChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            StreachableTextBox textBlock = sender as StreachableTextBox;
            textBlock.TBContent = (string)e.NewValue;
        } 
        #endregion

        #region ShouldFocuse
        public static readonly DependencyProperty ShouldFocuseProperty = DependencyProperty.RegisterAttached("ShouldFocuse", typeof(bool), typeof(StreachableTextBox), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        public bool ShouldFocuse
        {
            get { return (bool)GetValue(ShouldFocuseProperty); }
            set { SetValue(ShouldFocuseProperty, value); }
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
