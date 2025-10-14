using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SP.Parking.Terminal.Wpf.Utility
{
	// http://daveraine.blogspot.com/2013/05/data-binding-static-resource-in-wpf.html
	public class StringToIconConverter : IValueConverter
	{
		private static readonly ResourceDictionary _iconResource;
		private static readonly Dictionary<string, Geometry> _pathData;

		static StringToIconConverter()
		{
			if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			{
				_iconResource = new ResourceDictionary
				{
					Source = new Uri("pack://application:,,,/Resources/Icons.xaml")
				};

				_pathData = new Dictionary<string, Geometry>();
			}
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var resourceKey = value as string;

			return string.IsNullOrWhiteSpace(resourceKey) ? null : GetIconPathData(resourceKey);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Try and get the icon path data for the given resource key.
		/// </summary>
		/// <remarks>
		/// This method assumes Icons.xaml from MahApps.Metro is used, and that
		/// the icon resource is a Canvas containing a Path.
		/// </remarks>
		/// <param name="resourceKey">The resource key for the icon.</param>
		/// <returns>the icon path data if found, otherwise null.</returns>
		private static object GetIconPathData(string resourceKey)
		{
			if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
			{
				Geometry iconPathData;
				if (!_pathData.TryGetValue(resourceKey, out iconPathData))
				{
					Canvas iconCanvas = _iconResource[resourceKey] as Canvas;
					return iconCanvas;

					if (iconCanvas != null)
					{
						Path iconPath = iconCanvas.Children.Count > 0 ? iconCanvas.Children[0] as Path : null;

						if (iconPath != null)
						{
							iconPathData = iconPath.Data;
						}
					}

					_pathData.Add(resourceKey, iconPathData);
				}
				return iconPathData;
			}

			return null;
		}
	}
}
