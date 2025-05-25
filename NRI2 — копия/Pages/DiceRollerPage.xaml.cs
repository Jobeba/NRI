using Microsoft.Extensions.DependencyInjection;
using NRI.Classes;
using NRI.DiceRoll;
using NRI.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace NRI.Pages
{
    public partial class DiceRollerPage : Page
    {
        private readonly IAuthService _authService;
        private readonly JwtService _jwtService;
        private readonly Random _random = new Random();
        private const int AnimationDurationMs = 2000;
        private const double MaxAnimationScale = 1.5;

        private readonly Dictionary<string, DiceConfig> _diceConfigs = new Dictionary<string, DiceConfig>
        {
            ["D4"] = new DiceConfig(4, "D4", result => $"D4: {result}"),
            ["D6"] = new DiceConfig(6, "D6", result => $"D6: {result}"),
            ["D8"] = new DiceConfig(8, "D8", result => $"D8: {result}"),
            ["D10"] = new DiceConfig(10, "D10", result => $"D10: {result}"),
            ["D12"] = new DiceConfig(12, "D12", result => $"D12: {result}"),
            ["D20"] = new DiceConfig(20, "D20", result => $"D20: {result}"),
            ["D100"] = new DiceConfig(100, "D100", result => $"D100: {result}")
        };

        public DiceRollerPage(IAuthService authService, JwtService jwtService)
        {
            InitializeComponent();
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            var viewModel = App.ServiceProvider.GetRequiredService<DiceRollerViewModel>();

            if (Application.Current.Properties.Contains("CurrentUser"))
            {
                viewModel.CurrentUser = (User)Application.Current.Properties["CurrentUser"];
            }
            DataContext = viewModel;
            viewModel.LoadRollHistoryForCharacter();
            CheckAuthentication();
        }


        private void CheckAuthentication()
        {
            if (!_authService.IsUserAuthenticated())
            {
                MessageBox.Show("Для доступа к этой странице необходимо авторизоваться");
                // Перенаправление на страницу авторизации
                var authWindow = App.ServiceProvider.GetRequiredService<Autorizatsaya>();
                authWindow.Show();

                // Закрытие текущего окна, если это возможно
                if (Window.GetWindow(this) is Window window)
                {
                    window.Close();
                }
            }
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

                var dieTextBlock = new TextBlock
                {
                    Text = dieValue.ToString(),
                    Style = (Style)FindResource("DiceResultStyle")
                };

                itemsControl.Items.Add(dieTextBlock);
            }

            // Создаем сообщение без использования несуществующей переменной roll
            string rollMessage = $"Бросок {diceCount}{config.Name}: {string.Join(", ", results)} = {sum}";

            resultSumText.Text = config.FormatSum(sum);
            resultValuesText.Text = $"Значения: {string.Join(", ", results)}";
        }


        private string GenerateRollMessage(DiceRolling roll)
        {
            var result = roll.ResultsString;
            var total = roll.Total;
            var message = roll.CriticalMessage;

            return $"{roll.CharacterName} бросил {roll.DiceCount}d{roll.DiceType} " +
                   $"+ {roll.Modifier}: {result} = {total} " +
                   (string.IsNullOrEmpty(message) ? "" : $"\n{message}");
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

        public class DiceConfig
        {
            public int MaxValue { get; }
            public string Name { get; }
            public Func<int, string> FormatSum { get; }

            public DiceConfig(int maxValue, string name, Func<int, string> formatSum)
            {
                MaxValue = maxValue;
                Name = name;
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
