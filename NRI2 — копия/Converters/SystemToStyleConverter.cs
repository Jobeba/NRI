using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NRI.Converters
{
    namespace NRI.Converters
    {
        public class DiceStyleConverter : IValueConverter
        {
            public Style DnDStyle { get; set; }
            public Style PathfinderStyle { get; set; }
            public Style CthulhuStyle { get; set; }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is string system)
                {
                    return system switch
                    {
                        "D&D 5e" => DnDStyle,
                        "Pathfinder" => PathfinderStyle,
                        "Call of Cthulhu" => CthulhuStyle,
                        _ => null
                    };
                }
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
