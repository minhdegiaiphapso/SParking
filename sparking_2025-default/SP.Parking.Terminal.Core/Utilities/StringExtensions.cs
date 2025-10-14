using System;
using Cirrious.MvvmCross.Localization;

namespace SP.Parking.Terminal.Core
{
	public static class StringExtensions
	{
		public static string LocalisedString( this string str, IMvxLanguageBinder textSource )
		{
			return textSource.GetText(str);
		}
	}
}

