using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NRI.Classes
{
    public class NumericValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string strValue = value.ToString().Replace(" ", "");
            if (strValue.Length == 0)
                return new ValidationResult(false, "Введите код");

            foreach (char c in strValue)
            {
                if (!char.IsDigit(c))
                    return new ValidationResult(false, "Только цифры разрешены");
            }

            return ValidationResult.ValidResult;
        }
    }

}
