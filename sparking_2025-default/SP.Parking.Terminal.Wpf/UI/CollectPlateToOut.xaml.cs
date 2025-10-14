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
	/// Interaction logic for CollectPlateToOut.xaml
	/// </summary>
	public partial class CollectPlateToOut : Window
	{
		public CollectPlateToOut()
		{
			InitializeComponent();
			txtPlate.Text = "";
			Loaded += TestWindow_Loaded;
		}
		public CollectPlateToOut(string title, string plate = null)
		{
			InitializeComponent();
			if (plate != null)
			{
				txtPlate.Text = plate;
			}
			else
			{
				txtPlate.Text = "";
			}
			Loaded += TestWindow_Loaded;
			lblTitle.Text = title;
		}
		private void TestWindow_Loaded(object sender, RoutedEventArgs e)
		{
			txtPlate.Focusable = true;
			txtPlate.Focus();
		}
		public void SetForCus()
		{
			txtPlate.Focusable = true;
			txtPlate.Focus();
		}
		public string PlateText
		{
			get { return txtPlate.Text; }
			set { txtPlate.Text = value; }
		}
		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(this.PlateText) && this.PlateText.Length >= 7 && this.PlateText.Length <= 11)
			{
				DialogResult = true;
				this.Close();
			}
		}
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			this.Close();
		}
		private void txtPlate_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					if (!string.IsNullOrEmpty(this.PlateText) && this.PlateText.Length >= 7 && this.PlateText.Length <= 11)
					{
						DialogResult = true;
						this.Close();
					}
					break;
				case Key.Escape:
					DialogResult = false;
					this.Close();
					break;
			}
		}
	}
}
