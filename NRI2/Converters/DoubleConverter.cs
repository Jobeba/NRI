using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NRI.Converters
{
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), out double result))
                return result;
            return DependencyProperty.UnsetValue;
        }
    }
}
