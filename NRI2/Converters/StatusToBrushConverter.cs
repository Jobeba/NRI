using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NRI.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string colorString)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorString));
            }

            if (value == null)
                return Brushes.Gray;

            string status = value.ToString().ToLower();

            switch (status)
            {
                case "active":
                case "активен":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green

                case "inactive":
                case "неактивен":
                    return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow

                case "banned":
                case "заблокирован":
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red

                case "pending":
                case "ожидание":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue

                case "premium":
                case "премиум":
                    return new SolidColorBrush(Color.FromRgb(156, 39, 176)); // Purple

                default:
                    return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
