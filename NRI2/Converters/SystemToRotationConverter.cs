using System;
using System.Globalization;
using System.Windows.Data;

namespace NRI.Converters
{
    public class RotationConverter : IValueConverter
    {
        public int NormalRotation { get; set; } = 360;
        public int CriticalRotation { get; set; } = 720;
        public int ReverseRotation { get; set; } = -360;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DiceRolling roll)
            {
                if (roll.IsCriticalFailure)
                    return ReverseRotation;
                if (roll.IsCriticalSuccess)
                    return CriticalRotation;
                return NormalRotation;
            }
            return NormalRotation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
