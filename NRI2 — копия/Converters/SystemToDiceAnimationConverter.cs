using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NRI.Converters
{
    public class DiceAnimationConverter : IMultiValueConverter
    {
        // Обычные анимации
        public Style DnDAnimation { get; set; }
        public Style PathfinderAnimation { get; set; }
        public Style CthulhuAnimation { get; set; }

        // Критические анимации
        public Style DnDCriticalSuccessAnimation { get; set; }
        public Style PathfinderCriticalSuccessAnimation { get; set; }
        public Style CthulhuCriticalSuccessAnimation { get; set; }
        public Style DnDCriticalFailureAnimation { get; set; }
        public Style PathfinderCriticalFailureAnimation { get; set; }
        public Style CthulhuCriticalFailureAnimation { get; set; }

        // Эпические анимации
        public Style DnDEpicAnimation { get; set; }
        public Style PathfinderEpicAnimation { get; set; }
        public Style CthulhuEpicAnimation { get; set; }

        public Style DefaultStyle { get; set; }


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 ||
                !(values[0] is string system) ||
                !(values[1] is DiceRolling roll))
                return DefaultStyle;

            // Определяем "мощность" броска для выбора анимации
            bool isEpic = roll.Total >= roll.MaxPossible * 0.9 ||  // 90% от максимума
                          roll.Total <= roll.MinPossible * 1.1;    // 10% от минимума

            if (roll.IsCriticalSuccess)
            {
                return isEpic
                    ? GetEpicStyle(system)
                    : system switch
                    {
                        "D&D 5e" => DnDCriticalSuccessAnimation ?? DnDAnimation,
                        "Pathfinder" => PathfinderCriticalSuccessAnimation ?? PathfinderAnimation,
                        "Call of Cthulhu" => CthulhuCriticalSuccessAnimation ?? CthulhuAnimation,
                        _ => DefaultStyle
                    };
            }
            else if (roll.IsCriticalFailure)
            {
                return isEpic
                    ? GetEpicStyle(system)
                    : system switch
                    {
                        "D&D 5e" => DnDCriticalFailureAnimation ?? DnDAnimation,
                        "Pathfinder" => PathfinderCriticalFailureAnimation ?? PathfinderAnimation,
                        "Call of Cthulhu" => CthulhuCriticalFailureAnimation ?? CthulhuAnimation,
                        _ => DefaultStyle
                    };
            }
            else
            {
                return isEpic
                    ? GetEpicStyle(system)
                    : system switch
                    {
                        "D&D 5e" => DnDAnimation,
                        "Pathfinder" => PathfinderAnimation,
                        "Call of Cthulhu" => CthulhuAnimation,
                        _ => DefaultStyle
                    };
            }
        }

        private Style GetEpicStyle(string system)
        {
            return system switch
            {
                "D&D 5e" => DnDEpicAnimation ?? DnDCriticalSuccessAnimation ?? DnDAnimation,
                "Pathfinder" => PathfinderEpicAnimation ?? PathfinderCriticalSuccessAnimation ?? PathfinderAnimation,
                "Call of Cthulhu" => CthulhuEpicAnimation ?? CthulhuCriticalSuccessAnimation ?? CthulhuAnimation,
                _ => DefaultStyle
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
