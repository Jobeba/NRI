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
            if (value is string system)
            {
                var iconPath = system switch
                {
                    "D&D 5e" => "pack://application:,,,/Resources/dnd_icon.jpg",
                    "Pathfinder" => "pack://application:,,,/Resources/pathfinder_icon.png",
                    "Call of Cthulhu" => "pack://application:,,,/Resources/cthulhu_icon.jpg",
                    _ => "pack://application:,,,/Resources/default_icon.jpg"
                };
                return new BitmapImage(new Uri(iconPath));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
