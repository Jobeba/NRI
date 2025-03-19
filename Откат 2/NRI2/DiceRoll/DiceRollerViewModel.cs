using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System.Media;


namespace NRI.DiceRoll
{
    public class DiceRollerViewModel : INotifyPropertyChanged
    {
        private readonly Random _random = new Random();
        private DiceRolling _selectedRoll;
        private string _diceType;
        private int _diceCount;

        private double _animationDuration = 1000;
        private double _maxAnimationScale = 1.5;

        public double AnimationDuration
        {
            get => _animationDuration;
            set
            {
                _animationDuration = value;
                OnPropertyChanged();
            }
        }
        // История бросков
        public ObservableCollection<DiceRolling> RollHistory { get; } = new ObservableCollection<DiceRolling>();

        // Список доступных типов кубиков
        public ObservableCollection<string> DiceTypes { get; } = new ObservableCollection<string>
    {
        "D4", "D6", "D8", "D10", "D12", "D20", "D100"
    };

        // Выбранный бросок (для отображения деталей)
        public DiceRolling SelectedRoll
        {
            get => _selectedRoll;
            set
            {
                _selectedRoll = value;
                OnPropertyChanged();
            }
        }

        // Выбранный тип кубика
        public string DiceType
        {
            get => _diceType;
            set
            {
                _diceType = value;
                OnPropertyChanged();
            }
        }

        // Количество кубиков
        public int DiceCount
        {
            get => _diceCount;
            set
            {
                if (value < 0)
                {
                    MessageBox.Show("Количество кубиков не может быть отрицательным.");
                    return;
                }
                _diceCount = value;
                OnPropertyChanged();
            }
        }

        // Команда для броска кубиков
        public ICommand RollDiceCommand => new RelayCommand(RollDice);

        // Команда для очистки истории
        public ICommand ClearHistoryCommand => new RelayCommand(ClearHistory);

        // Логика броска кубиков
        private void RollDice()
        {
            if (DiceCount <= 0)
            {
                MessageBox.Show("Введите корректное количество кубиков.");
                return;
            }

            var results = new List<int>();
            int sum = 0;

            for (int i = 0; i < DiceCount; i++)
            {
                int dieValue = _random.Next(1, GetMaxValue(DiceType) + 1);
                results.Add(dieValue);
                sum += dieValue;
            }

            var roll = new DiceRolling
            {
                DiceType = DiceType,
                DiceCount = DiceCount,
                Results = results,
                Sum = sum
            };

            RollHistory.Insert(0, roll); // Добавляем бросок в историю
            SelectedRoll = roll; // Выбираем текущий бросок для отображения
        }

        // Очистка истории бросков
        private void ClearHistory()
        {
            RollHistory.Clear();
        }

        // Получение максимального значения для типа кубика
            private int GetMaxValue(string diceType)
            {
                switch (diceType)
                {
                    case "D4": return 4;
                    case "D6": return 6;
                    case "D8": return 8;
                    case "D10": return 10;
                    case "D12": return 12;
                    case "D20": return 20;
                    case "D100": return 100;
                    default: throw new ArgumentException("Неизвестный тип кубика");
                }
            }
        

        // Уведомление об изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void PlaySound()
        {
         try
            {
            // Путь к звуковому файлу
            var soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Music/Roll_the_dice.mp3");

            // Воспроизведение звука
            var player = new SoundPlayer(soundPath);
            player.Play();
             }
             catch (Exception ex)
            {
                // Обработка ошибок (например, если файл не найден)
                MessageBox.Show($"Ошибка воспроизведения звука: {ex.Message}");
            }
    }


        public double MaxAnimationScale
        {
            get => _maxAnimationScale;
            set
            {
                _maxAnimationScale = value;
                OnPropertyChanged();
            }
        }
    }
}
