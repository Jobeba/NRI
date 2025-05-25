using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NRI.Converters
{
    public class SystemToColorConverter : IValueConverter
    {
        public Color DnDColor { get; set; }
        public Color PathfinderColor { get; set; }
        public Color CthulhuColor { get; set; }
        public Color WarhammerColor { get; set; }
        public Color GURPSColor { get; set; }
        public Color FATEColor { get; set; }
        public Color DefaultColor { get; set; } = Colors.Gray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string system)
            {
                return system switch
                {
                    "D&D 5e" => DnDColor,
                    "Pathfinder" => PathfinderColor,
                    "Call of Cthulhu" => CthulhuColor,
                    "Warhammer" => WarhammerColor,
                    "GURPS" => GURPSColor,
                    "FATE" => FATEColor,
                    _ => DefaultColor
                };
            }
            return DefaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SystemToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string system)
            {
                var colorKey = $"{system.Replace(" ", "")}Color";
                var color = Application.Current.TryFindResource(colorKey) as Color? ?? Colors.Transparent;

                if (parameter is string variant)
                {
                    switch (variant)
                    {
                        case "Light":
                            return new SolidColorBrush(ChangeColorBrightness(color, 0.4f));
                        case "Hover":
                            return new SolidColorBrush(ChangeColorBrightness(color, 0.2f));
                        case "Dark":
                            return new SolidColorBrush(ChangeColorBrightness(color, -0.3f));
                    }
                }

                return new SolidColorBrush(color);
            }
            return Brushes.Transparent;
        }

        private Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R / 255f;
            float green = color.G / 255f;
            float blue = color.B / 255f;

            if (correctionFactor > 0)
            {
                red = (1 - red) * correctionFactor + red;
                green = (1 - green) * correctionFactor + green;
                blue = (1 - blue) * correctionFactor + blue;
            }
            else
            {
                red = (1 + correctionFactor) * red;
                green = (1 + correctionFactor) * green;
                blue = (1 + correctionFactor) * blue;
            }

            return Color.FromArgb(color.A,
                (byte)(red * 255),
                (byte)(green * 255),
                (byte)(blue * 255));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
