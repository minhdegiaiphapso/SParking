using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SP.Parking.Terminal.Wpf.Services
{
    public class LocalizeService : ILocalizeService
    {
        public string GetText(string key)
        {
            return (string)Application.Current.FindResource(key);
        }
    }
}
