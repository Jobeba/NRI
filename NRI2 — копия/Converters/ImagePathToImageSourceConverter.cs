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
    public class ImagePathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default_character.jpg"));

            try
            {
                return new BitmapImage(new Uri(value.ToString()));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/default_character.jpg"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
