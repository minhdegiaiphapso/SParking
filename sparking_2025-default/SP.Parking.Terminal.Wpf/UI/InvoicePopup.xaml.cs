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
using System.Windows.Shapes;

namespace SP.Parking.Terminal.Wpf.UI
{
	/// <summary>
	/// Interaction logic for InvoicePopup.xaml
	/// </summary>
	public partial class InvoicePopup : Window
	{
		public InvoicePopup()
		{
			InitializeComponent();
			this.KeyUp += new KeyEventHandler(OnKeyup);
		}

		private void OnKeyup(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					DialogResult = true;
					this.Close();
					break;
				case Key.Escape:
					DialogResult = false;
					this.Close();
					break;
			}
		}

		public string BuyerCode
		{
			get { return txtCode.Text; }
			set { txtCode.Text = value; }
		}
		public string BuyerName
		{
			get { return txtName.Text; }
			set { txtName.Text = value; }
		}
		public string LegalName
		{
			get { return txtLegalName.Text; }
			set { txtLegalName.Text = value; }
		}
		public string TaxCode
		{
			get { return txtTaxcode.Text; }
			set { txtTaxcode.Text = value; }
		}

		public string Phone
		{
			get { return txtPhone.Text; }
			set { txtPhone.Text = value; }
		}
		public string Address
		{
			get { return txtAddress.Text; }
			set { txtAddress.Text = value; }
		}
		public string Email
		{
			get { return txtEmail.Text; }
			set { txtEmail.Text = value; }
		}
		public string ReceiverName
		{
			get { return txtReceiverName.Text; }
			set { txtReceiverName.Text = value; }
		}
		public string ReceiverEmails
		{
			get { return txtReceiverMails.Text; }
			set { txtReceiverMails.Text = value; }
		}
		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			this.Close();
		}
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			this.Close();
		}
	}
}
