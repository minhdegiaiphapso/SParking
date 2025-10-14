using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SP.Parking.Terminal.Wpf.Utility
{
    public class TextboxValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return new ValidationResult(false, "Please enter some text");
            }
            return new ValidationResult(true, null);
        }
    }
}
