using Cirrious.CrossCore.Converters;
using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SP.Parking.Terminal.Core.Utilities
{
    public class ByteImageValueConverter : MvxValueConverter<byte[], ImageSource>
    {
        BitmapImage biImg = null;
        protected override ImageSource Convert(byte[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return null;
                using (MemoryStream ms = new MemoryStream(value))
                {
                    biImg = new BitmapImage();
                    biImg.BeginInit();
                    biImg.CacheOption = BitmapCacheOption.OnLoad;
                    biImg.StreamSource = ms;
                    biImg.EndInit();
                    //biImg.Freeze();
                    //if (biImg.StreamSource == null) return null;
                    ImageSource imgSrc = biImg as ImageSource;

                    return imgSrc;
                }
            }
            catch { return null; }
        }
    }
}