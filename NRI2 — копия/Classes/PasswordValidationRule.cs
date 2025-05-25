using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NRI.Classes
{
    public class PasswordValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo culture)
        {
            var password = (string)value;
            var strength = CalculateStrength(password);
            if (strength < 50) // Пример: сложность меньше 50%
                return new ValidationResult(false, "Пароль слишком слабый");
            return ValidationResult.ValidResult;
        }

        private int CalculateStrength(string password)
        {
            // Логика расчета сложности (длина, символы, цифры)
            return password.Length * 10 + (password.Any(char.IsDigit) ? 20 : 0);
        }
    }
}
