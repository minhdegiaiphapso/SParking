using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for VoucherView.xaml
    /// </summary>
    public partial class VoucherView : BaseView
    {
        public VoucherView()
        {
            InitializeComponent();
        }
        public void ResetText()
        {
            try
            {
                txt24.Text = "";
                txt4.Text = "";
                txtMoney.Text = "";
            }
            catch
            {

            }
        }
        public override void ViewLoaded(object sender, RoutedEventArgs e)
        {
            base.ViewLoaded(sender, e);

        }
        //
        public int VehicleTypeId { get; set; }
        public Core.Models.Voucher data { get; set; }
        public CheckOutLaneViewModel model { get; set; }
        public bool CanVoucher { get; set; }
        public bool IsVoucher { get; set; }
        public string CardIDVC { get; set; }
        public int SecondVC { get; set; }
        public void ShowText()
        {
            if (data != null)
            {
                lblFee.Text = data.Parking_Fee.ToString();
                lblActualFee.Text = data.Actual_Fee.ToString();
                lblVoucherAmount.Text = data.Voucher_Amount.ToString();
            }
        }
        public void VoucherByTime()
        {
            if (CanVoucher)
            {
                int num4 = 0;
                int num24 = 0;
                int.TryParse(txt4.Text, out num4);
                int.TryParse(txt24.Text, out num24);
                int totalhour = num4 * 4 + num24 * 24;
                if (totalhour > 0 && model!=null && SecondVC>0)
                {          
                    string vtype = "";
                    if (num4 > 0)
                    {
                       
                        if (num24 > 0)
                        {
                           
                            vtype = string.Format("{0} Voucher 4h and {1} Voucher 24h", num4, num24);
                        }
                        else
                        {
                            vtype = string.Format("{0} Voucher 4h", num4);
                        }
                    }
                    else
                    {
                        vtype = string.Format("{0} Voucher 24h", num24);
                    }
                    data.Voucher_Type = vtype;  
                    int Hours = (SecondVC / 3600) + (SecondVC % 3600 > 0 ? 1 : 0);
                    if (totalhour >= Hours)
                    {
                        data.Actual_Fee = 0;
                        data.Voucher_Amount = data.Parking_Fee;
                    }
                    else
                    {
                        int recall =  model.Recallfee(CardIDVC, totalhour);
                        if (recall > 0 && data.Parking_Fee >= data.Actual_Fee)
                        {
                            data.Actual_Fee = recall;
                            if (data.Actual_Fee > data.Parking_Fee)
                                data.Actual_Fee = data.Parking_Fee;
                            data.Voucher_Amount = data.Parking_Fee - data.Actual_Fee;
                        }
                    }
                    IsVoucher = true;
                }
                else
                {
                    data.Actual_Fee = data.Parking_Fee;
                    data.Voucher_Type = "";
                    data.Voucher_Amount = 0;
                    IsVoucher = false;
                }
                ShowText();
            }
        }
        public void VoucherByFOC()
        {
            if (CanVoucher)
            {
                IsVoucher = true;
                data.Actual_Fee = 0;
                data.Voucher_Amount = data.Parking_Fee;
                data.Voucher_Type = "FOC";
                IsVoucher = true;
                ShowText();
            }
        }
        public void VoucherByEvent()
        {
            if (CanVoucher)
            {
                int amount = -1;
                int.TryParse(txtMoney.Text, out amount);
                if (amount != -1)
                {
                    IsVoucher = true;
                    data.Actual_Fee = amount;
                    if (data.Actual_Fee > data.Parking_Fee)
                        data.Actual_Fee = data.Parking_Fee;
                    data.Voucher_Amount = data.Parking_Fee - data.Actual_Fee;
                    data.Voucher_Type = "Event";

                }
                else
                {
                    data.Actual_Fee = data.Parking_Fee;
                    data.Voucher_Amount = data.Parking_Fee - data.Actual_Fee;
                    data.Voucher_Type = "Event";
                }
                IsVoucher = true;
                ShowText();
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton r = sender as RadioButton;

            try
            {
                Grid g = (r.Parent as WrapPanel).Parent as Grid;
                Grid Byhour = g.FindName("Byhour") as Grid;
                if (Byhour != null)
                {
                    Grid FOC = g.FindName("FOC") as Grid;
                    Grid byevent = g.FindName("byevent") as Grid;
                    Byhour.Visibility = Visibility.Visible;
                    FOC.Visibility = Visibility.Hidden;
                    byevent.Visibility = Visibility.Hidden;
                    VoucherByTime();
                }
            }
            catch
            {

            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton r = sender as RadioButton;
                Grid g = (r.Parent as WrapPanel).Parent as Grid;
                Grid Byhour = g.FindName("Byhour") as Grid;
                Grid FOC = g.FindName("FOC") as Grid;
                Grid byevent = g.FindName("byevent") as Grid;
                Byhour.Visibility = Visibility.Hidden;
                FOC.Visibility = Visibility.Visible;
                byevent.Visibility = Visibility.Hidden;
                VoucherByFOC();
            }
            catch
            { }
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton r = sender as RadioButton;
                Grid g = (r.Parent as WrapPanel).Parent as Grid;
                Grid Byhour = g.FindName("Byhour") as Grid;
                Grid FOC = g.FindName("FOC") as Grid;
                Grid byevent = g.FindName("byevent") as Grid;
                Byhour.Visibility = Visibility.Hidden;
                FOC.Visibility = Visibility.Hidden;
                byevent.Visibility = Visibility.Visible;
                VoucherByEvent();
            }
            catch
            { }
        }
        private void Closeme()
        {
            if (IsVoucher && model != null && data != null)
                 model.Voucher(data);
            this.Visibility = Visibility.Hidden;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Closeme();
            if (this.model != null)
                this.model.IsVoucher = false;
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            if (this.model != null)
            {
                //if(data != null && !string.IsNullOrEmpty(model.CardIDVC)&& model.CheckInTime!=null)
                //{
                //    model.DeleteVoucher(model.CardIDVC, model.CheckInTime);
                //}
                this.model.IsVoucher = false;
            }
        }
        private void txt4_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                VoucherByTime();
            }
            catch
            {; }
        }

        private void txt24_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                VoucherByTime();
            }
            catch
            {; }
        }
        private void txtMoney_TextChanged(object sender, TextChangedEventArgs e)
        {
            VoucherByEvent();
        }

        private void BaseView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                Closeme();
                //this.model.IsVoucher = false;
            }
            if (e.Key == Key.C)
            {
                this.Visibility = Visibility.Hidden;
                //this.model.IsVoucher = false;
            }
        } 

        private void txt4_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txt24_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch
            {
                TextBox txt = sender as TextBox;
                txt.Text = "";
            }
        }

        private void txtMoney_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch
            {
                TextBox txt = sender as TextBox;
                txt.Text = "";
            }
        }
        //
    }
}
