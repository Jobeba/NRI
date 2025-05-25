using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Data;

namespace NRI.Converters
{
    public class SystemToStyleConverter : IValueConverter
    {
        // Добавим стили для всех систем
        public Style DnDStyle { get; set; }
        public Style PathfinderStyle { get; set; }
        public Style CthulhuStyle { get; set; }
        public Style DefaultStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string system)
            {
                return system switch
                {
                    "D&D 5e" => DnDStyle,
                    "Pathfinder" => PathfinderStyle,
                    "Call of Cthulhu" => CthulhuStyle,
                    _ => DefaultStyle
                };
            }
            return DefaultStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
