using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NRI.Converters
{
    public class SystemToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var system = value as string;
            return system switch
            {
                "D&D 5e" => Color.FromRgb(123, 31, 162),   // Фиолетовый
                "Pathfinder" => Color.FromRgb(211, 47, 47), // Красный
                "Call of Cthulhu" => Color.FromRgb(25, 118, 210), // Синий
                _ => Color.FromRgb(97, 97, 97)             // Серый
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class SystemToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorConverter = new SystemToColorConverter();
            var color = (Color)colorConverter.Convert(value, targetType, parameter, culture);
            return new SolidColorBrush(color);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
