using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NRI.Converters
{
    public class SystemToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                "D&D 5e" => new BitmapImage(new Uri("pack://application:,,,/Resources/dnd_icon.jpeg")),
                "Pathfinder" => new BitmapImage(new Uri("pack://application:,,,/Resources/pathfinder_icon.png")),
                "Call of Cthulhu" => new BitmapImage(new Uri("pack://application:,,,/Resources/cthulhu_icon.jpeg")),
                _ => new BitmapImage(new Uri("pack://application:,,,/Resources/default_icon.jpeg"))
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование не требуется, поэтому просто возвращаем null
            return null;
        }
    }
}
