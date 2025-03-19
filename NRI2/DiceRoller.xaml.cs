using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NRI
{
    public partial class DiceRoller : Window
    {
        private readonly Random _random = new Random();
        private const int AnimationDurationMs = 2000; // Длительность анимации в миллисекундах
        private const double MaxAnimationScale = 1.5; // Максимальный масштаб анимации

        // Конфигурация для каждого типа кубика
        private readonly Dictionary<string, DiceConfig> _diceConfigs = new Dictionary<string, DiceConfig>
        {
            ["D4"] = new DiceConfig(4, result => $"D4: {result}"),
            ["D6"] = new DiceConfig(6, result => $"D6: {result}"),
            ["D8"] = new DiceConfig(8, result => $"D8: {result}"),
            ["D10"] = new DiceConfig(10, result => $"D10: {result}"),
            ["D12"] = new DiceConfig(12, result => $"D12: {result}"),
            ["D20"] = new DiceConfig(20, result => $"D20: {result}"),
            ["D100"] = new DiceConfig(100, result => $"D100: {result}")
        };

        public DiceRoller()
        {
            InitializeComponent();
        }

        private void RollDice_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var diceType = button.Tag.ToString();

            if (!_diceConfigs.TryGetValue(diceType, out var config)) return;

            var countTextBox = FindName($"{diceType}CountTextBox") as TextBox;
            var itemsControl = FindName($"{diceType}ItemsControl") as ItemsControl;
            var resultSumText = FindName($"{diceType}ResultSum") as TextBlock;
            var resultValuesText = FindName($"{diceType}ResultValues") as TextBlock;

            if (!int.TryParse(countTextBox?.Text, out var diceCount) || diceCount <= 0)
            {
                MessageBox.Show("Введите корректное количество кубиков.");
                return;
            }

            RollDice(config, diceCount, itemsControl, resultSumText, resultValuesText);
        }

        private void RollDice(DiceConfig config, int diceCount, ItemsControl itemsControl,
                            TextBlock resultSumText, TextBlock resultValuesText)
        {
            var results = new List<int>();
            int sum = 0;

            itemsControl.Items.Clear();

            for (int i = 0; i < diceCount; i++)
            {
                int dieValue = _random.Next(1, config.MaxValue + 1);
                results.Add(dieValue);
                sum += dieValue;

                var dieTextBlock = CreateDieTextBlock(dieValue.ToString());
                AnimateDieRoll(dieTextBlock);
                itemsControl.Items.Add(dieTextBlock);
            }

            resultSumText.Text = config.FormatSum(sum);
            resultValuesText.Text = $"Значения: {string.Join(", ", results)}";
        }

        private TextBlock CreateDieTextBlock(string value)
        {
            return new TextBlock
            {
                Text = value,
                FontSize = 24,
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(2),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TransformGroup() // Используем TransformGroup
                {
                    Children = new TransformCollection
                    {
                        new RotateTransform(), // Вращение
                        new ScaleTransform()  // Масштабирование
                    }
                }
            };
        }

        private void AnimateDieRoll(TextBlock dieTextBlock)
        {
            if (dieTextBlock.RenderTransform is TransformGroup transformGroup)
            {
                // Анимация вращения
                var rotateAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                    AutoReverse = false
                };

                // Анимация масштабирования
                var scaleAnimation = new DoubleAnimation
                {
                    From = 0.5,
                    To = MaxAnimationScale,
                    Duration = TimeSpan.FromMilliseconds(AnimationDurationMs),
                    AutoReverse = true,
                    EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 4 }
                };

                // Применяем анимацию к RotateTransform и ScaleTransform
                var rotateTransform = transformGroup.Children[0] as RotateTransform;
                var scaleTransform = transformGroup.Children[1] as ScaleTransform;

                rotateTransform?.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                scaleTransform?.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform?.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
        }

        private void ClearResults_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var diceType = button.Tag.ToString();

            if (FindName($"{diceType}ResultSum") is TextBlock resultSumText)
                resultSumText.Text = string.Empty;

            if (FindName($"{diceType}ResultValues") is TextBlock resultValuesText)
                resultValuesText.Text = string.Empty;
        }

        private class DiceConfig
        {
            public int MaxValue { get; }
            public Func<int, string> FormatSum { get; }

            public DiceConfig(int maxValue, Func<int, string> formatSum)
            {
                MaxValue = maxValue;
                FormatSum = formatSum;
            }
        }

        public class MillisecondsToTimeSpanConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double milliseconds)
                {
                    return TimeSpan.FromMilliseconds(milliseconds);
                }
                return TimeSpan.Zero;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}