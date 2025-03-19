using NRI.Classes;
using System.Collections.Generic;
using System;
using System.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace NRI.Classes // Убедитесь, что пространство имен совпадает с указанным в XAML
{
    public class DiceRollerViewModel : INotifyPropertyChanged
    {
        private readonly Random _random = new Random();
        private DiceRoll _selectedRoll;
        private string _diceType;
        private int _diceCount;

        public ObservableCollection<DiceRoll> RollHistory { get; } = new ObservableCollection<DiceRoll>();
        public ObservableCollection<string> DiceTypes { get; } = new ObservableCollection<string> { "D4", "D6", "D8", "D10", "D12", "D20", "D100" };

        public DiceRoll SelectedRoll
        {
            get => _selectedRoll;
            set
            {
                _selectedRoll = value;
                OnPropertyChanged();
            }
        }

        public string DiceType
        {
            get => _diceType;
            set
            {
                _diceType = value;
                OnPropertyChanged();
            }
        }

        public int DiceCount
        {
            get => _diceCount;
            set
            {
                _diceCount = value;
                OnPropertyChanged();
            }
        }
        public ICommand RollDiceCommand => new RelayCommand(RollDice);
        public ICommand ClearHistoryCommand => new RelayCommand(ClearHistory);

        private void RollDice()
        {
            if (DiceCount <= 0)
            {
                MessageBox.Show("Введите корректное количество кубиков.");
                return;
            }

            // Воспроизведение звука
            PlaySound();

            var results = new List<int>();
            int sum = 0;

            for (int i = 0; i < DiceCount; i++)
            {
                int dieValue = _random.Next(1, GetMaxValue(DiceType) + 1);
                results.Add(dieValue);
                sum += dieValue;
            }

            var roll = new DiceRoll
            {
                DiceType = DiceType,
                DiceCount = DiceCount,
                Results = results,
                Sum = sum
            };

            RollHistory.Insert(0, roll);
            SelectedRoll = roll;
        }
        private void PlaySound()
        {
            try
            {
                // Путь к звуковому файлу
                var soundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "dice_roll.wav");

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
        private void ClearHistory()
        {
            RollHistory.Clear();
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}