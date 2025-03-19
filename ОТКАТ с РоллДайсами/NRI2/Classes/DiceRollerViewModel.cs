using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using System.Resources;
using System.Drawing;

namespace NRI.Classes
{
    
public class DiceResult
{
    public int Value { get; set; }
    public ImageSource ImageSource { get; set; }
}

public class RollResult
{
    public string DiceType { get; set; }
    public List<DiceResult> DiceResults { get; set; }
    public int Sum => DiceResults.Sum(d => d.Value);
    public DateTime Timestamp { get; set; }
}

    public class DiceRollerViewModel : INotifyPropertyChanged
    {
        private readonly Random _random = new Random();

        public ObservableCollection<string> DiceTypes { get; } = new ObservableCollection<string>
    {
        "D4", "D6", "D8", "D10", "D12", "D20", "D100"
    };

        private string _diceType;
        public string DiceType
        {
            get => _diceType;
            set
            {
                _diceType = value;
                OnPropertyChanged();
            }
        }

        private int _diceCount = 1;
        public int DiceCount
        {
            get => _diceCount;
            set
            {
                _diceCount = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RollResult> RollHistory { get; } = new ObservableCollection<RollResult>();

        private RollResult _selectedRoll;
        public RollResult SelectedRoll
        {
            get => _selectedRoll;
            set
            {
                _selectedRoll = value;
                OnPropertyChanged();
            }
        }

        public ICommand RollDiceCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public DiceRollerViewModel()
        {
            RollDiceCommand = new RelayCommand<string>(RollDice);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            DiceType = DiceTypes.FirstOrDefault();
        }

        private void RollDice(string diceType)
        {
            if (string.IsNullOrEmpty(diceType)) return;

            var diceResults = new List<DiceResult>();
            for (int i = 0; i < DiceCount; i++)
            {
                int maxValue = int.Parse(diceType.Substring(1));

                int value = _random.Next(1, maxValue + 1);
                diceResults.Add(new DiceResult
                {
                    Value = value,
                    ImageSource = GetDiceImageSource(diceType, value)
                });
            }

            var rollResult = new RollResult
            {
                DiceType = diceType,
                DiceResults = diceResults,
                Timestamp = DateTime.Now
            };

            RollHistory.Add(rollResult);
            SelectedRoll = rollResult;
        }
        private ImageSource GetDiceImageSource(string diceType, int value)
        {
            // Создаём ResourceManager для файла ресурсов
            var resourceManager = new ResourceManager("NRI.Properties.Images", typeof(DiceRoller).Assembly);

            // Формируем имя ресурса
            string resourceName = $"{diceType.ToLower()}_{value}";

            try
            {
                // Пытаемся загрузить ресурс как массив байтов
                var imageBytes = (byte[])resourceManager.GetObject(resourceName);

                if (imageBytes != null)
                {
                    // Конвертируем массив байтов в Bitmap
                    using (var memoryStream = new System.IO.MemoryStream(imageBytes))
                    {
                        var bitmap = new Bitmap(memoryStream);
                        return ConvertBitmapToImageSource(bitmap);
                    }
                }
                else
                {
                    // Логируем, что ресурс не найден
                    Console.WriteLine($"Ресурс '{resourceName}' не найден.");
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка загрузки ресурса '{resourceName}': {ex.Message}");
            }

            // Если ресурс не найден, возвращаем заглушку
            return new BitmapImage(new Uri("pack://application:,,,/NRI;component/image/D10.jpg", UriKind.Absolute));
        }

        // Метод для конвертации Bitmap в ImageSource
        private ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void ClearHistory()
        {
            RollHistory.Clear();
            SelectedRoll = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}