using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using SP.Parking.Terminal.Core.Models;
using System.Drawing;
using System.Windows.Media;

namespace SP.Parking.Terminal.Wpf.Utility
{
    public class BoolVisiblilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isVisible = (bool)value;
            return isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Visible;
        }
    }

    public class LevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int level = (int)value;
            switch(level)
            {
                case 1:
                    return System.Windows.Media.Brushes.DarkGreen;
                case 2:
                    return System.Windows.Media.Brushes.DarkBlue;
                case 3:
                    return System.Windows.Media.Brushes.DarkRed;
                default:
                    return System.Windows.Media.Brushes.Black;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }

    public class StringCodeToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            string code = (string)value;
            return (string)Application.Current.FindResource(code);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    //public class EnumToBooleanConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        LaneDirection direction = (LaneDirection)Enum.Parse(typeof(LaneDirection), parameter.ToString());
    //        LaneDirection valDirection = (LaneDirection)Enum.Parse(typeof(LaneDirection), value.ToString());

    //        if (valDirection == direction) return true;
    //        else return false;

    //        //return ((Enum)value).HasFlag((Enum)parameter);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value.Equals(true) ? parameter : Binding.DoNothing;
    //    }
    //}

	public class StringColorToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch ((string)value)
			{
				case "Red":
					return System.Windows.Media.Brushes.Red;

                case "Cyan":
                    return System.Windows.Media.Brushes.Cyan;

				case "LightGreen":
					return System.Windows.Media.Brushes.LightGreen;
                case "Green":
                    return System.Windows.Media.Brushes.Green;
                case "DarkGreen":
                    return System.Windows.Media.Brushes.DarkGreen;
                case "LightBlue":
                    return System.Windows.Media.Brushes.LightBlue;
                case "Blue":
                    return System.Windows.Media.Brushes.Blue;
                case "DarkBlue":
                    return System.Windows.Media.Brushes.DarkBlue;
                case "Yellow":
					return System.Windows.Media.Brushes.Yellow;

				case "Orange":
					return System.Windows.Media.Brushes.Orange;

                case "White":
                    return System.Windows.Media.Brushes.White;
			}

			return System.Windows.Media.Brushes.White;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return 0;
		}
	}

    public class StringToEnableConverter : IValueConverter
    {
        private const string REMOTE_CARD_READER = "Remote Card Reader";
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (!string.IsNullOrEmpty(str) && str.Equals(REMOTE_CARD_READER))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        private const string REMOTE_CARD_READER = "Remote Card Reader";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (!string.IsNullOrEmpty(str))
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
