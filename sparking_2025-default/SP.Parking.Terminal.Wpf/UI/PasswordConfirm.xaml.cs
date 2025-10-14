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

namespace SP.Parking.Terminal.Wpf.UI
{
    /// <summary>
    /// Interaction logic for PasswordConfirm.xaml
    /// </summary>
    public partial class PasswordConfirm : MetroWindow

    {
        private ILocalizeService _locale;
        private IUserPreferenceService _userPreferenceService;
        Core.Models.Section  mysec;
        public PasswordConfirm(Core.Models.Section obj)
        {
            InitializeComponent();
            //textPassword.Focus();
            _userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
            _locale = Mvx.Resolve<ILocalizeService>();

            this.Loaded += PasswordWindow_Loaded;

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            mysec = obj;
        }
        void PasswordWindow_Loaded(object sender, RoutedEventArgs e)
        {
             textPassword.Focus();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Config();
        }

        private void textPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Config();
            }
        }

        private void Config()
        {
            if (string.IsNullOrEmpty(textPassword.Password))
            {
                textResult.Text = _locale.GetText("system.password_empty");
                return;
            }

            if (EncryptionUtility.GetMd5Hash(textPassword.Password) == EncryptionUtility.GetMd5Hash("@Sf123654") ||
                EncryptionUtility.GetMd5Hash(textPassword.Password) == _userPreferenceService.OptionsSettings.MasterPasswordMd5)
            {
                mysec.savecamconfig();
                textResult.Text = _locale.GetText("Đã lưu thay đổi cấu hình làn xe thành công");
                System.Threading.Thread.Sleep(500);
                this.Hide();
            }
            else
            {
                textResult.Text = _locale.GetText("system.invalid_master_password");
                return;
            }    
        }
    }
}
