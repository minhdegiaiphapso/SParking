using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using MahApps.Metro.Controls;
using SP.Parking.Terminal.Core.Services;
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
using System.Windows.Shapes;
using SP.Parking.Terminal.Core.Models.Custom;

namespace SP.Parking.Terminal.Wpf.UI
{
	/// <summary>
	/// Interaction logic for PasswordWindow.xaml
	/// </summary>
	public partial class AddRegisteredCard : MetroWindow
	{
		private ILocalizeService _locale;
		private IUserPreferenceService _userPreferenceService;
		private IServer _server;

		public event EventHandler MapCardSuccess;

		public string CardId { get; set; }

		public AddRegisteredCard()
		{
			InitializeComponent();

			_userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
			_locale = Mvx.Resolve<ILocalizeService>();
			_server = Mvx.Resolve<IServer>();

			this.Loaded += PasswordWindow_Loaded;

			this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
		}

		void PasswordWindow_Loaded(object sender, RoutedEventArgs e)
		{
			textResult.Text = "";
			textResult.Visibility = Visibility.Collapsed;
			txtVehicleNumber.Focus();
		}

		private void HandleEsc(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		private void ConfigButton_Click(object sender, RoutedEventArgs e)
		{
			
		}

		private void textPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				RequestMapping();
			}
		}

        private void RequestMapping()
        {
			this.Invoke(new Action(() =>
			{
				textResult.Text = "Requesting...";
				textResult.Visibility = Visibility.Visible;
			}));
				
			if (string.IsNullOrEmpty(txtVehicleNumber.Text))
            {
                textResult.Text = _locale.GetText("system.vehicle_number_empty");
                return;
            }

			_server.AddRegisteredCard(CardId, txtVehicleNumber.Text, (result, ex) =>
			{
				var addRegisterCardResult = result as ApiResponseModel<object>;
				if(addRegisterCardResult != null)
                {
					if(addRegisterCardResult.Success)
                    {
						this.Hide();

						var handle = MapCardSuccess;
						if (handle != null)
							handle(null, null);
					}
                    else
                    {
						textResult.Text = addRegisterCardResult.ErrorMessage;
					}
                }
                else
                {
					textResult.Text = _locale.GetText("system.error_occured");
				}
			});

			
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
			RequestMapping();
		}

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
			Close();
		}
    }
}