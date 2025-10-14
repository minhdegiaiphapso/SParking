using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Parking.Terminal.Core.Services;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro;
using ControlzEx.Theming;

namespace SP.Parking.Terminal.Wpf.Services
{
    public class UIService : IUIService
    {
        public void ShowMessage(MessageToUser message)
        {
            var window = Application.Current.MainWindow as MetroWindow;
            var dialog = (BaseMetroDialog)window.Resources["SimpleDialogTest"];
            MessageDialogStyle style = MessageDialogStyle.Affirmative;
            MetroDialogSettings settings = new MetroDialogSettings();
            switch (message.Options.Count)
            {
                case 1:
                    style = MessageDialogStyle.Affirmative;
                    settings.AffirmativeButtonText = message.Options[0].Title;
                    break;
                case 2:
                    style = MessageDialogStyle.AffirmativeAndNegative;
                    foreach( var option in message.Options)
                    {
                        if(option.Status == MessageToUser.OptionStatus.Positive)
                            settings.AffirmativeButtonText = option.Title;
                        else if (option.Status == MessageToUser.OptionStatus.Negative || option.Status == MessageToUser.OptionStatus.Neutral)
                            settings.NegativeButtonText = option.Title;
                    }
                    break;
                case 3:
                    style = MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary;
                    foreach (var option in message.Options)
                    {
                        if (option.Status == MessageToUser.OptionStatus.Positive)
                            settings.AffirmativeButtonText = option.Title;
                        else if (option.Status == MessageToUser.OptionStatus.Negative)
                            settings.NegativeButtonText = option.Title;
                        else if (option.Status == MessageToUser.OptionStatus.Neutral)
                            settings.FirstAuxiliaryButtonText = option.Title;
                    }
                    break;
            }


            window.ShowMessageAsync(message.Title, message.Message, style, settings);
        }

        public void OpenUrl(string url)
        {
            throw new NotImplementedException();
        }

        public void ChangeColor(string color)
        {
            if (!string.IsNullOrEmpty(color))
            {
                //var accent = ThemeManager..First(x => x.Name == color);
                //var theme = ThemeManager.AppThemes.FirstOrDefault(x => x.Name == "BaseLight");
                ThemeManager.Current.ChangeTheme(Application.Current, color);
            }
        }
    }
}
