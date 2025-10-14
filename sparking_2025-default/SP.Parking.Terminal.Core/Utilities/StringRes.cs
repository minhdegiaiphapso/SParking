using Cirrious.MvvmCross.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SP.Parking.Terminal.Core.Utilities
{
    public class StringRes
    {
        public string GroupKey { get; set; }

        public StringRes(object type)
            : this(type.GetType())
        {
        }

        public StringRes()
        {
        }

        public StringRes(Type type)
        {
            string typeName = type.Name;
            int findUnderScore = typeName.LastIndexOf("_");
            if (findUnderScore >= 0)
                typeName = typeName.Substring(0, findUnderScore);
            this.GroupKey = typeName.Replace("ViewModel", "").Replace("View", "");
        }

        public string GetText(string key)
        {
            string result = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(key))
                    return "";

                if (key.IndexOf('.') >= 0)
                    return (string)Application.Current.FindResource(key); 

                if (string.IsNullOrEmpty(GroupKey))
                    result = (string)Application.Current.FindResource(key);
                else
                    result = (string)Application.Current.FindResource(GroupKey + "." + key);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

		public static string GetButtonText(string key)
		{
			return (string)Application.Current.FindResource("button." + key);
		}

		public static string GetCommonText(string key)
		{
			return (string)Application.Current.FindResource("common." + key);
		}
    }
}
