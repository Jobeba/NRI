using GalaSoft.MvvmLight.CommandWpf;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using NLog;
using NRI.Classes;
using NRI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NRI.Windows;
using Newtonsoft.Json;

namespace NRI.DiceRoll
{
    public partial class DiceRollerViewModel : INotifyPropertyChanged
    {
        private SystemTemplate _currentTemplate;
        private SystemTemplate _template;
        private CharacterSheet _selectedCharacter;
        private readonly ILogger _logger;
        private readonly IConfigService _configService;
        private readonly IAuthService _authService;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        private User _currentUser;
        private readonly Random _random = new Random();
        private DiceRolling _selectedRoll;
        private CharacterSheet _currentCharacter;
        private bool _isEditing;
        private string _diceType;
        private int _diceCount = 1;
        private int _modifier;
        private string _characterName;
        private string _selectedSystem = "D&D 5e";
        private double _animationDuration = 1000;
        private double _maxAnimationScale = 1.5;
        public ICommand RollDiceCommand => new RelayCommand(RollDice);
        public ICommand ClearHistoryCommand => new RelayCommand(ClearHistory);
        public ICommand AddCharacterCommand => new RelayCommand(AddCharacter);
        public ICommand EditCharacterCommand => new RelayCommand<CharacterSheet>(EditCharacter);
        public ICommand SaveCharacterCommand => new AsyncRelayCommand(SaveCurrentCharacterAsync);
        public ICommand DeleteCharacterCommand => new RelayCommand<CharacterSheet>(async (character) =>
        {
            if (character == null) return;

            await DeleteCharacterAsync(character);
        });

        public ICommand PrintCurrentCharacterCommand => new RelayCommand(PrintCurrentCharacter);
        public ICommand LoadImageCommand => new RelayCommand(LoadCharacterImage);
        public ICommand CancelEditCommand => new RelayCommand(CancelEdit);
        public ICommand AddAttributeCommand => new RelayCommand(AddAttribute);
        public ICommand AddSkillCommand => new RelayCommand(AddSkill);
        public ICommand AddInventoryItemCommand { get; }
        public ICommand RemoveInventoryItemCommand { get; }
        public ICommand RemoveAttributeCommand => new RelayCommand<CharacterAttributeItem>(RemoveAttribute);
        public ICommand RemoveSkillCommand => new RelayCommand<CharacterSkillItem>(RemoveSkill);

        public ObservableCollection<DiceSet> DiceSets { get; } = new ObservableCollection<DiceSet>();
        public class DiceSet
        {
            public string DiceType { get; set; }
            public int Count { get; set; }
        }
        public ICommand AddDiceSetCommand => new RelayCommand(AddDiceSet);
        public ICommand RemoveDiceSetCommand => new RelayCommand<DiceSet>(RemoveDiceSet);
        public ICommand RollCustomDiceCommand => new RelayCommand(RollCustomDice);

        private void AddDiceSet()
        {
            DiceSets.Add(new DiceSet { DiceType = DiceTypes.FirstOrDefault() ?? "D20", Count = 1 });
        }

        private void RemoveDiceSet(DiceSet diceSet)
        {
            if (diceSet != null)
            {
                DiceSets.Remove(diceSet);
            }
        }

        private async void RollCustomDice()
        {
            if (!DiceSets.Any())
            {
                MessageBox.Show("Добавьте хотя бы один тип кубиков");
                return;
            }

            var rolls = new List<DiceRolling>();
            var results = new List<string>();

            foreach (var diceSet in DiceSets)
            {
                var roll = await RollDiceInternal(diceSet.DiceType, diceSet.Count, 0);
                rolls.Add(roll);
                results.Add($"{diceSet.Count}{diceSet.DiceType}: {string.Join(", ", roll.Results)} (Итого: {roll.Total})");
            }

            var combinedRoll = new DiceRolling
            {
                CharacterName = SelectedCharacter?.Name ?? "Без персонажа",
                UserId = _currentUser?.Id ?? 0, // Добавьте проверку на null
                CharacterId = SelectedCharacter?.CharacterID, // Оставьте null если персонажа нет
                Results = rolls.SelectMany(r => r.Results).ToList(),
                DiceType = "Custom",
                DiceCount = rolls.Sum(r => r.DiceCount),
                Modifier = 0,
                CustomDescription = string.Join(" + ", results)
            };

            await SaveRollToDatabase(combinedRoll);
            RollHistory.Insert(0, combinedRoll);
            SelectedRoll = combinedRoll;
        }

        private async Task<DiceRolling> RollDiceInternal(string diceType, int diceCount, int modifier)
        {
            var results = new List<int>();

            for (int i = 0; i < diceCount; i++)
            {
                int value = diceType == "FATE" ? _random.Next(-1, 2) : _random.Next(1, GetMaxValue(diceType) + 1);
                results.Add(value);
            }

            var roll = new DiceRolling
            {
                DiceType = DiceType,
                DiceCount = DiceCount,
                Modifier = Modifier,
                Results = results,
                CharacterName = SelectedCharacter?.Name ?? "Без персонажа",
                UserId = _currentUser?.Id ?? 0,
                CharacterId = SelectedCharacter?.CharacterID // Может быть null
                                                             // Исключите присвоение Character = SelectedCharacter
            };

            await SaveRollToDatabase(roll);
            return roll;
        }

        private bool _showCustomDice = false;
        public bool ShowCustomDice
        {
            get => _showCustomDice;
            set
            {
                _showCustomDice = value;
                OnPropertyChanged();
            }
        }

        private async void RollDice()
        {
            if (string.IsNullOrEmpty(DiceType))
            {
                MessageBox.Show("Выберите тип кубика");
                return;
            }

            // Воспроизведение звука броска
            PlayDiceRollSound();

            var results = new List<int>();

            for (int i = 0; i < DiceCount; i++)
            {
                int value = DiceType == "FATE" ?
                    _random.Next(-1, 2) : // FATE dice: -1, 0, +1
                    _random.Next(1, GetMaxValue(DiceType) + 1);

                results.Add(value);
            }

            var roll = new DiceRolling
            {
                DiceType = DiceType,
                DiceCount = DiceCount,
                Modifier = Modifier,
                Results = results,
                Character = SelectedCharacter, // Убедитесь, что это свойство правильно установлено
                CharacterName = SelectedCharacter?.Name ?? "Без персонажа",
                UserId = _currentUser.Id,
                CharacterId = SelectedCharacter?.CharacterID
            };

            await SaveRollToDatabase(roll);
            RollHistory.Insert(0, roll);
            SelectedRoll = roll;

            // Проверяем критические броски для всех систем
            if (roll.IsCriticalSuccess)
            {
                PlayCriticalSound(true);
            }
            else if (roll.IsCriticalFailure)
            {
                PlayCriticalSound(false);
            }
        }

        private void PlayDiceRollSound()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Resources/dice_roll.wav", UriKind.Absolute);
                var player = new MediaPlayer();
                player.Open(uri);
                player.Play();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка воспроизведения звука броска кубиков");
            }
        }


        public class AsyncRelayCommand : ICommand
        {
            private readonly Func<Task> _execute;
            private readonly Func<bool> _canExecute;

            public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

            public async void Execute(object parameter)
            {
                await _execute();
            }
        }

        public DiceRollerViewModel(IAuthService authService, DbContextOptions<AppDbContext> dbOptions, ILogger logger = null)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _dbOptions = dbOptions ?? throw new ArgumentNullException(nameof(dbOptions));
            _logger = logger ?? LogManager.GetCurrentClassLogger();
            _logger.Info("Инициализация DiceRollerViewModel");



            AddInventoryItemCommand = new RelayCommand(AddInventoryItem);
            RemoveInventoryItemCommand = new RelayCommand<InventoryItem>(RemoveInventoryItem);

            _logger.Info($"Загружено шаблонов систем: {_systemTemplates.Count}");
            foreach (var system in _systemTemplates.Keys)
            {
                _logger.Info($"Доступна система: {system}");
            }

            // Инициализируем CurrentCharacter перед подпиской на события
            CurrentCharacter = new CharacterSheet();
            CurrentTemplate = new SystemTemplate();
            // Теперь можно подписаться на события
            CurrentCharacter.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CurrentCharacter.System))
                {
                    ApplySystemTemplateToCharacter(CurrentCharacter);
                }
            };

            // Проверяем аутентификацию перед загрузкой данных
            if (!_authService.IsUserAuthenticated())
            {
                throw new UnauthorizedAccessException("Пользователь не аутентифицирован");
            }

            _currentUser = _authService.GetCurrentUser();
            if (_currentUser == null)
            {
                throw new InvalidOperationException("Не удалось получить данные пользователя");
            }

            // Проверяем существование пользователя в БД с использованием временного контекста
            using (var context = new AppDbContext(_dbOptions))
            {
                if (!context.Users.Any(u => u.Id == _currentUser.Id))
                {
                    throw new InvalidOperationException("Пользователь не найден в базе данных");
                }
            }

            DiceType = DiceTypes.FirstOrDefault();
            LoadCharactersAsync().ConfigureAwait(false);
        }

        public bool IsAuthenticated => _currentUser != null &&
                         !string.IsNullOrEmpty(_currentUser.Token);
        public ObservableCollection<DiceRolling> RollHistory { get; } = new ObservableCollection<DiceRolling>();

        public ObservableCollection<string> DiceTypes { get; } = new ObservableCollection<string>
        {
            "D2", "D3", "D4", "D6", "D8", "D10", "D12", "D20", "D100", "FATE"
        };

        public ObservableCollection<string> GameSystems { get; } = new ObservableCollection<string>
        {
            "D&D 5e", "Pathfinder", "Call of Cthulhu", "Warhammer", "GURPS", "FATE"
        };

        public CharacterSheet CurrentCharacter
        {
            get => _currentCharacter;
            set
            {
                _currentCharacter = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<CharacterSheet> CharacterSheets { get; } = new ObservableCollection<CharacterSheet>();

        public DiceRolling SelectedRoll
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
                _diceCount = value > 0 ? value : 1;
                OnPropertyChanged();
            }
        }

        public int Modifier
        {
            get => _modifier;
            set
            {
                _modifier = value;
                OnPropertyChanged();
            }
        }

        public string CharacterName
        {
            get => _characterName;
            set
            {
                _characterName = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSystem
        {
            get => _selectedSystem;
            set
            {
                if (_selectedSystem != value)
                {
                    _selectedSystem = value;
                    OnPropertyChanged();

                    if (IsEditing && CurrentCharacter != null)
                    {
                        var result = MessageBox.Show(
                            "Применить шаблон новой системы? Все текущие данные будут сброшены.",
                            "Подтверждение",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            CurrentCharacter.System = value;
                            ApplySystemTemplateToCharacter(CurrentCharacter);
                        }
                    }
                }
            }
        }

        public double AnimationDuration
        {
            get => _animationDuration;
            set
            {
                _animationDuration = value;
                OnPropertyChanged();
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

        private async Task LoadCharactersAsync()
        {
            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    var characters = await context.Characters
                        .Where(c => c.UserId == _currentUser.Id)
                        .AsNoTracking()
                        .ToListAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        CharacterSheets.Clear();
                        foreach (var character in characters)
                        {
                            // Десериализация JSON произойдет автоматически через свойства
                            CharacterSheets.Add(character);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка загрузки персонажей");
                MessageBox.Show($"Ошибка загрузки персонажей: {ex.Message}");
            }
        }

        private async Task SaveCurrentCharacterAsync()
        {
            if (CurrentCharacter == null) return;

            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    bool isNew = CurrentCharacter.CharacterID == 0;

                    // Ensure all collections are properly serialized
                    CurrentCharacter.SerializeCollections();

                    if (isNew)
                    {
                        context.Characters.Add(CurrentCharacter);
                    }
                    else
                    {
                        var existing = await context.Characters
                            .FirstOrDefaultAsync(c => c.CharacterID == CurrentCharacter.CharacterID);
                        if (existing != null)
                        {
                            // Force update of all properties including JSON fields
                            context.Entry(existing).CurrentValues.SetValues(CurrentCharacter);

                            // Explicitly mark as modified to ensure update
                            context.Entry(existing).State = EntityState.Modified;

                            
                            existing.Notes = CurrentCharacter.Notes;
                            existing.PersonalityTraits = CurrentCharacter.PersonalityTraits;
                            existing.Ideals = CurrentCharacter.Ideals;
                            existing.Bonds = CurrentCharacter.Bonds;
                            existing.Flaws = CurrentCharacter.Flaws;
                            existing.Appearance = CurrentCharacter.Appearance;
                            existing.Backstory = CurrentCharacter.Backstory;

                            // Force update of JSON fields
                            existing.AttributesJson = CurrentCharacter.AttributesJson;
                            existing.SkillsJson = CurrentCharacter.SkillsJson;
                            existing.InventoryJson = CurrentCharacter.InventoryJson;

                        }
                    }

                    CurrentCharacter.LastModified = DateTime.Now;
                    await context.SaveChangesAsync();

                    MessageBox.Show("Персонаж успешно сохранен");
                    await LoadCharactersAsync();
                    IsEditing = false;
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка сохранения персонажа");
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }


        public enum CharacterTrackedEntityState
        {
            Added,
            Modified,
            Deleted,
            Unchanged
        }
        private void EditCharacter(CharacterSheet character)
        {
            if (character == null) return;

            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    var loadedCharacter = context.Characters
                        .AsNoTracking()
                        .FirstOrDefault(c => c.CharacterID == character.CharacterID);

                    if (loadedCharacter != null)
                    {
                        // Десериализуем JSON свойства
                        loadedCharacter.DeserializeCollections();

                        // Явно обновляем JSON свойства после десериализации
                        loadedCharacter.UpdateAttributesJson();
                        loadedCharacter.UpdateSkillsJson();
                        loadedCharacter.UpdateInventoryJson();

                        CurrentCharacter = loadedCharacter;
                        ApplySystemTemplateToCharacter(CurrentCharacter);
                        IsEditing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка загрузки персонажа");
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }


        private void CancelEdit()
        {
            CurrentCharacter = new CharacterSheet();
            IsEditing = false;
        }

        private void ApplySystemTemplate()
        {
            if (CurrentCharacter == null || string.IsNullOrEmpty(CurrentCharacter.System))
                return;

            if (_systemTemplates.TryGetValue(CurrentCharacter.System, out var template))
            {
                CurrentTemplate = template;

                // Явно обновляем все свойства
                OnPropertyChanged(nameof(CurrentTemplate));
                OnPropertyChanged(nameof(CurrentCharacter));

                // Вместо вызова OnPropertyChanged для каждого таба, просто уведомляем об изменении коллекции Tabs
                OnPropertyChanged(nameof(CurrentTemplate.Tabs));
            }
        }



        private void LoadCharacterImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение персонажа"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentCharacter.ImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(CurrentCharacter));
            }
        }

        private void ResetCurrentCharacter()
        {
            CurrentCharacter = new CharacterSheet();
            IsEditing = false;
        }

        private async Task SaveRollToDatabase(DiceRolling roll)
        {
            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    // Ensure we're not trying to save a new character along with the roll
                    if (roll.Character != null && roll.Character.CharacterID != 0)
                    {
                        // Attach the existing character to prevent insertion
                        context.Characters.Attach(roll.Character);
                    }

                    if (roll.RollId == 0)
                    {
                        context.DiceRolls.Add(roll);
                    }
                    else
                    {
                        context.DiceRolls.Update(roll);
                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка сохранения броска в БД");
                MessageBox.Show($"Ошибка сохранения броска: {ex.Message}");
            }
        }

        private void PlayCriticalSound(bool isSuccess)
        {
            if (SelectedCharacter == null) return;

            try
            {
                string soundName = isSuccess ? "critical_success" : "critical_fail";
                var uri = new Uri($"pack://application:,,,/Resources/{soundName}.wav");
                var player = new MediaPlayer();
                player.Open(uri);
                player.Play();

                string message;
                switch (SelectedCharacter.System)
                {
                    case "Call of Cthulhu":
                        message = isSuccess ?
                            "🎉 Критический успех! (1)" :
                            "💥 Критический провал! (100)";
                        break;
                    case "D&D 5e":
                    case "Pathfinder":
                    default:
                        message = isSuccess ?
                            "🎉 Критический успех! (20)" :
                            "💥 Критический провал! (1)";
                        break;
                }

                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    MessageBox.Show(message, "Критический бросок",
                        MessageBoxButton.OK,
                        isSuccess ? MessageBoxImage.Exclamation : MessageBoxImage.Hand);
                });
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка воспроизведения звука");
            }
        }

        private int GetMaxValue(string diceType)
        {
            return diceType switch
            {
                "D2" => 2,
                "D3" => 3,
                "D4" => 4,
                "D6" => 6,
                "D8" => 8,
                "D10" => 10,
                "D12" => 12,
                "D20" => 20,
                "D100" => 100,
                _ => throw new ArgumentException("Неизвестный тип кубика")
            };
        }

        private async void ClearHistory()
        {
            if (SelectedCharacter == null)
            {
                MessageBox.Show("Выберите персонажа для очистки истории");
                return;
            }

            var result = MessageBox.Show(
                $"Вы действительно хотите очистить историю бросков для персонажа '{SelectedCharacter.Name}'?",
                "Подтверждение очистки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    var rollsToDelete = await context.DiceRolls
                        .Where(r => r.CharacterId == SelectedCharacter.CharacterID)
                        .ToListAsync();

                    context.DiceRolls.RemoveRange(rollsToDelete);
                    await context.SaveChangesAsync();

                    RollHistory.Clear();
                    MessageBox.Show("История бросков очищена");
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка очистки истории бросков");
                MessageBox.Show($"Ошибка очистки истории бросков: {ex.Message}");
            }
        }

        private readonly Dictionary<string, SystemTemplate> _systemTemplates = new()
        {
            ["D&D 5e"] = new SystemTemplate
            {
                SystemName = "D&D 5e",
                Tabs = new ObservableCollection<SystemTab>
                {
                    // Вкладка характеристик
                    new SystemTab
                    {
                        Header = "Характеристики",
                        AttributeGroups = new ObservableCollection<AttributeGroup>
                        {
                        new AttributeGroup
                            {
                                GroupName = "Основные характеристики",
                                Columns = 3,
                                Attributes = new ObservableCollection<CharacterAttributeItem>
                                {
                                    new() { Name = "Сила", Value = "10" },
                                    new() { Name = "Ловкость", Value = "10" },
                                    new() { Name = "Телосложение", Value = "10" },
                                    new() { Name = "Интеллект", Value = "10" },
                                    new() { Name = "Мудрость", Value = "10" },
                                    new() { Name = "Харизма", Value = "10" }
                                }
                            },
                                new AttributeGroup
                                {
                                    GroupName = "Здоровье и защита",
                                    Columns = 1,
                                    Attributes = new ObservableCollection<CharacterAttributeItem>
                                {
                                    new() { Name = "Класс доспеха", Value = "10" },
                                    new() { Name = "Инициатива", Value = "0" },
                                    new() { Name = "Скорость", Value = "30" },
                                    new() { Name = "Макс. ХП", Value = "10" },
                                    new() { Name = "Текущие ХП", Value = "10" },
                                    new() { Name = "Временные ХП", Value = "0" }
                                }
                            }
                }
            },
            
            // Вкладка навыков
                    new SystemTab
                    {
                        Header = "Навыки",
                        Skills = new ObservableCollection<CharacterSkillItem>
                        {
                            new() { Name = "Акробатика (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Атлетика (Сил)", Value = "0", IsProficient = false },
                            new() { Name = "Внимательность (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Выживание (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Дрессировка (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Запугивание (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "Исполнение (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "История (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Ловкость рук (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Магия (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Медицина (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Обман (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "Природа (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Проницательность (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Религия (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Скрытность (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Убеждение (Хар)", Value = "0", IsProficient = false }
                        }
                    },
                      new SystemTab
                    {
                        Header = "Классовые особенности",
                        AdditionalFields = new ObservableDictionary
                        {
                            { "Архетип", "" },
                            { "Классовые способности", "" },
                            { "Черты", "" }
                        }
                    },

                // Вкладка инвентаря
                new SystemTab
            {
                Header = "Инвентарь",
                Inventory = new ObservableCollection<InventoryItem>
                {
                    new InventoryItem { Name = "Оружие", Quantity = 1 },
                    new InventoryItem { Name = "Доспех", Quantity = 1 },
                    new InventoryItem { Name = "Зелья лечения", Quantity = 3 }
                }
            },
            
            // Вкладка дополнительной информации
            new SystemTab
            {
                 Header = "Дополнительно",
                 AdditionalFields = new ObservableDictionary
                {
                    { "Мировоззрение", "Нейтральное" },
                    { "Божество", "Нет" },
                    { "Языки", "Общий" },
                    { "Особенности", "" },
                    { "Черты характера", "" },
                    { "Идеалы", "" },
                    { "Привязанности", "" },
                    { "Слабости", "" }
                }
            },
            
            // Вкладка заметок
                    new SystemTab
                    {
                        Header = "Заметки",
                        Notes = "Здесь можно записывать важную информацию о персонаже, квестах и событиях."
                    }
                },

                        Races = new ObservableCollection<string>
                {
                    "Человек", "Эльф", "Дварф", "Полурослик",
                    "Тифлинг", "Драконорождённый", "Гном",
                    "Полуэльф", "Полуорк"
                },

                 Classes = new ObservableCollection<string>
                {
                    "Воин", "Волшебник", "Жрец", "Плут",
                    "Паладин", "Следопыт", "Друид", "Варвар",
                    "Монах", "Чародей", "Колдун", "Бард", 
                    "Изобретатель","Кровавый охотник", "Военачальник","Егерь",
                    "Звездочёт", "Магус", "Мистик", "Неупокоенная душа",
                    "Савант", "Хранитель ран", "Шаман"
                },

                        TemplateImagePath = "pack://application:,,,/Resources/dnd_template.png"
                    },

            ["Pathfinder"] = new SystemTemplate
            {
                SystemName = "Pathfinder",
                Tabs = new ObservableCollection<SystemTab>
            {
                new SystemTab
                {
                    Header = "Характеристики",
                    AttributeGroups = new ObservableCollection<AttributeGroup>
                    {
                        new AttributeGroup
                        {
                                GroupName = "Основные характеристики",
                                Columns = 3,
                                Attributes = new ObservableCollection<CharacterAttributeItem>
                                {
                                    new() { Name = "Сила (STR)", Value = "10" },
                                    new() { Name = "Ловкость (DEX)", Value = "10" },
                                    new() { Name = "Телосложение (CON)", Value = "10" },
                                    new() { Name = "Интеллект (INT)", Value = "10" },
                                    new() { Name = "Мудрость (WIS)", Value = "10" },
                                    new() { Name = "Харизма (CHA)", Value = "10" },
                                    new() { Name = "Базовое атака", Value = "1" }
                                }
                            },
                            new AttributeGroup
                            {
                                GroupName = "Защита",
                                Columns = 2,
                                Attributes = new ObservableCollection<CharacterAttributeItem>
                                {
                                    new() { Name = "Класс брони (AC)", Value = "10" },
                                    new() { Name = "КМ (CMD)", Value = "0" },
                                    new() { Name = "КЗ (CMD)", Value = "10" },
                                    new() { Name = "Инициатива", Value = "0" }
                                }
                            },
                            new AttributeGroup
                            {
                                GroupName = "Здоровье",
                                Columns = 1,
                                Attributes = new ObservableCollection<CharacterAttributeItem>
                                {
                                    new() { Name = "Макс. HP", Value = "10" },
                                    new() { Name = "Текущие HP", Value = "10" },
                                    new() { Name = "Временные HP", Value = "0" },
                                    new() { Name = "Спасброски", Value = "0" }
                                }
                            }
                        }
                    },
                    new SystemTab
                    {
                        Header = "Навыки",
                        Skills = new ObservableCollection<CharacterSkillItem>
                        {
                            new() { Name = "Акробатика (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Атлетика (Сил)", Value = "0", IsProficient = false },
                            new() { Name = "Внимание (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Выживание (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Дипломатия (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "Запугивание (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "Знание (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Ловкость рук (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Магия (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Медицина (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Обман (Хар)", Value = "0", IsProficient = false },
                            new() { Name = "Природа (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Проницательность (Мдр)", Value = "0", IsProficient = false },
                            new() { Name = "Религия (Инт)", Value = "0", IsProficient = false },
                            new() { Name = "Скрытность (Лов)", Value = "0", IsProficient = false },
                            new() { Name = "Убеждение (Хар)", Value = "0", IsProficient = false }
                        }
                    },
                           new SystemTab
                                    {
                                        Header = "Способности",
                                        AdditionalFields = new ObservableDictionary
                                        {
                                            ["Расовые"] = "",
                                            ["Классовые"] = "",
                                            ["Черты"] = "",
                                            ["Заклинания"] = ""
                                        }
                                    },
                           new SystemTab
                           {
                                Header = "Инвентарь",
                                Inventory = new ObservableCollection<InventoryItem>
                                {
                                    new InventoryItem
                                    {
                                        Name = "Оружие",
                                        Quantity = 1,
                                        Description = "Основное оружие персонажа",
                                        IsEquipped = true
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Доспех",
                                        Quantity = 1,
                                        Description = "Основной доспех",
                                        IsEquipped = true
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Зелья лечения",
                                        Quantity = 3,
                                        Description = "Восстанавливают 2d4+2 HP"
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Набор путешественника",
                                        Quantity = 1,
                                        Description = "Включает палатку, котелок и другие принадлежности"
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Веревка (15м)",
                                        Quantity = 1,
                                        Description = "Пеньковая веревка, выдерживает до 200 кг"
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Факелы",
                                        Quantity = 3,
                                        Description = "Горят 1 час каждый"
                                    },
                                    new InventoryItem
                                    {
                                        Name = "Золото",
                                        Quantity = 100,
                                        Description = "Монеты в кошельке"
                                    }
                                }
                            },
                           new SystemTab
                            {
                                Header = "Заметки",
                                Notes = "Здесь можно записывать информацию о квестах, союзниках и важных событиях."
                            }
                                },
                           Races = new ObservableCollection<string>
                           {
                             "Человек", "Эльф", "Дварф", "Полурослик",
                             "Полуэльф", "Полуорк", "Гном", "Халфлинг"
                           },

                           Classes = new ObservableCollection<string>
                           {
                             "Воин", "Волшебник", "Жрец", "Плут",
                              "Паладин", "Следопыт", "Друид", "Варвар",
                              "Монах", "Оракул", "Алхимик", "Инквизитор"
                           },

                           TemplateImagePath = "pack://application:,,,/Resources/pathfinder_template.png"
                           },

                ["Call of Cthulhu"] = new SystemTemplate
                    {
                        SystemName = "Call of Cthulhu",
                        Tabs = new ObservableCollection<SystemTab>
                    {
                        new SystemTab
                        {
                            Header = "Характеристики",
                            AttributeGroups = new ObservableCollection<AttributeGroup>
                            {
                                new AttributeGroup
                                {
                                    GroupName = "Основные характеристики",
                                    Columns = 2,
                                    Attributes = new ObservableCollection<CharacterAttributeItem>
                                    {
                                        new() { Name = "Сила (STR)", Value = "50" },
                                        new() { Name = "Ловкость (DEX)", Value = "50" },
                                        new() { Name = "Телосложение (CON)", Value = "50" },
                                        new() { Name = "Интеллект (INT)", Value = "50" },
                                        new() { Name = "Мудрость (POW)", Value = "50" },
                                        new() { Name = "Харизма (CHA)", Value = "50" }
                                    }
                                },
                                new AttributeGroup
                                {
                                    GroupName = "Производные характеристики",
                                    Columns = 2,
                                    Attributes = new ObservableCollection<CharacterAttributeItem>
                                    {
                                        new() { Name = "Рассудок (SAN)", Value = "50" },
                                        new() { Name = "Текущий рассудок", Value = "50" },
                                        new() { Name = "Макс. рассудок", Value = "50" },
                                        new() { Name = "Порог безумия", Value = "20" },
                                        new() { Name = "Удача", Value = "50" },
                                        new() { Name = "Магия", Value = "10" },
                                        new() { Name = "Точность", Value = "25" },
                                        new() { Name = "Здоровье", Value = "10" }
                                    }
                                }
                            }
                        },
                        new SystemTab
                        {
                            Header = "Навыки",
                            Skills = new ObservableCollection<CharacterSkillItem>
                            {
                                new() { Name = "Антропология", Value = "1", IsProficient = false },
                                new() { Name = "Археология", Value = "1", IsProficient = false },
                                new() { Name = "Астрономия", Value = "1", IsProficient = false },
                                new() { Name = "Биология", Value = "1", IsProficient = false },
                                new() { Name = "Взлом", Value = "1", IsProficient = false },
                                new() { Name = "Выживание", Value = "10", IsProficient = false },
                                new() { Name = "Гипноз", Value = "1", IsProficient = false },
                                new() { Name = "Драка", Value = "25", IsProficient = false },
                                new() { Name = "Животноводство", Value = "5", IsProficient = false },
                                new() { Name = "Иностранный язык", Value = "1", IsProficient = false },
                                new() { Name = "История", Value = "5", IsProficient = false },
                                new() { Name = "Красноречие", Value = "5", IsProficient = false },
                                new() { Name = "Лёгкое оружие", Value = "20", IsProficient = false },
                                new() { Name = "Маскировка", Value = "5", IsProficient = false },
                                new() { Name = "Медицина", Value = "1", IsProficient = false },
                                new() { Name = "Пилотирование", Value = "1", IsProficient = false },
                                new() { Name = "Плавание", Value = "20", IsProficient = false },
                                new() { Name = "Психология", Value = "10", IsProficient = false },
                                new() { Name = "Скрытность", Value = "20", IsProficient = false },
                                new() { Name = "Стрельба", Value = "25", IsProficient = false }
                            }
                        },
                        new SystemTab
                        {
                            Header = "Оружие и снаряжение",
                            Inventory = new ObservableCollection<InventoryItem>
                            {
                                new InventoryItem { Name = "Пистолет (1d10)", Quantity = 1, Description = "Боеприпасы: 7/7" },
                                new InventoryItem { Name = "Нож (1d4)", Quantity = 1, Description = "Холодное оружие" },
                                new InventoryItem { Name = "Дробовик (2d6/1d10/1d4)", Quantity = 1, Description = "Боеприпасы: 2/2" },
                                new InventoryItem { Name = "Фонарь", Quantity = 1, Description = "Батареи: 3 часа" },
                                new InventoryItem { Name = "Аптечка", Quantity = 1, Description = "Использований: 3" }
                            }
                        },
                        new SystemTab
                        {
                            Header = "Личная информация",
                            AdditionalFields = new Dictionary<string, string>
                            {
                                ["Профессия"] = "",
                                ["Родной город"] = "",
                                ["Образование"] = "",
                                ["Возраст"] = "",
                                ["Пол"] = "",
                                ["Рост"] = "",
                                ["Вес"] = "",
                                ["Внешность"] = "",
                                ["Черты характера"] = "",
                                ["Фобии"] = "",
                                ["Травмы"] = "",
                                ["Финансы"] = "",
                                ["Личные вещи"] = ""
                            }.ToObservableDictionary()
                        },
                        new SystemTab
                        {
                            Header = "Заметки",
                            Notes = "Здесь можно записывать встреченных существ, потерю рассудка, важные события и другую информацию."
                        }
                        },
                                    Races = new ObservableCollection<string> { "Человек" },
                                    Classes = new ObservableCollection<string>
                        {
                            "Исследователь",
                            "Детектив",
                            "Учёный",
                            "Писатель",
                            "Художник",
                            "Врач",
                            "Криминалист",
                            "Журналист",
                            "Антиквар"
                        },
                            TemplateImagePath = "pack://application:,,,/Resources/coc_template.png"
                        }
            };

        public ObservableCollection<string> Alignments { get; } = new ObservableCollection<string>
        {
            "Законопослушный добрый",
            "Законопослушный нейтральный",
            "Законопослушный злой",
            "Нейтральный добрый",
            "Истинный нейтральный",
            "Нейтральный злой",
            "Хаотичный добрый",
            "Хаотичный нейтральный",
            "Хаотичный злой"
        };

        private void AddCharacter()
        {
            if (!_authService.IsUserAuthenticated())
            {
                MessageBox.Show("Войдите в систему для создания персонажа");
                return;
            }

            _currentUser = _authService.GetCurrentUser();

            if (_currentUser == null || _currentUser.Id == 0)
            {
                MessageBox.Show("Не удалось получить данные пользователя");
                return;
            }

            if (string.IsNullOrWhiteSpace(CharacterName))
            {
                MessageBox.Show("Введите имя персонажа");
                return;
            }

            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    if (!context.Users.Any(u => u.Id == _currentUser.Id))
                    {
                        MessageBox.Show("Пользователь не найден в базе данных");
                        return;
                    }

                    if (context.Characters.Any(c => c.UserId == _currentUser.Id && c.Name == CharacterName))
                    {
                        MessageBox.Show("Персонаж с таким именем уже существует");
                        return;
                    }

                    var character = new CharacterSheet
                    {
                        Name = CharacterName,
                        System = SelectedSystem,
                        UserId = _currentUser.Id,
                        CreatedDate = DateTime.Now,
                        LastModified = DateTime.Now
                    };

                    CurrentCharacter = character;
                    ApplySystemTemplateToCharacter(CurrentCharacter);

                    context.Characters.Add(character);
                    context.SaveChanges();

                    var loadedCharacter = context.Characters
                                    .FirstOrDefault(c => c.CharacterID == character.CharacterID);

                    if (loadedCharacter != null)
                    {
                        // Десериализуем JSON свойства
                        loadedCharacter.AttributesCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterAttributeItem>>(loadedCharacter.AttributesJson);
                        loadedCharacter.SkillsCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterSkillItem>>(loadedCharacter.SkillsJson);
                        loadedCharacter.InventoryItems = JsonConvert.DeserializeObject<ObservableCollection<InventoryItem>>(loadedCharacter.InventoryJson);

                        CurrentCharacter = loadedCharacter;
                        CharacterSheets.Add(CurrentCharacter);
                        IsEditing = true;
                        SelectedCharacter = CurrentCharacter;
                    }

                    if (loadedCharacter != null)
                    {
                        loadedCharacter.DeserializeCollections();
                        CurrentCharacter = loadedCharacter;
                        CharacterSheets.Add(CurrentCharacter);
                        IsEditing = true;
                        SelectedCharacter = CurrentCharacter;
                    }


                    CharacterName = string.Empty;
                    MessageBox.Show("Персонаж успешно создан");
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка сохранения персонажа: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void ApplySystemTemplateToCharacter(CharacterSheet character)
        {
            if (character == null || string.IsNullOrEmpty(character.System))
            {
                _logger?.Warn("Не удалось применить шаблон: персонаж или система не заданы");
                return;
            }

            if (_systemTemplates.TryGetValue(character.System, out var template))
            {
                CurrentTemplate = template;

                // Инициализируем персонажа из шаблона
                character.InitializeFromTemplate(template);

                OnPropertyChanged(nameof(CurrentTemplate));
                OnPropertyChanged(nameof(CurrentCharacter));
            }
        }


        private async Task DeleteCharacterAsync(CharacterSheet character)
        {
            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    // Сначала обнуляем CharacterId в связанных бросках
                    var rolls = await context.DiceRolls
                        .Where(r => r.CharacterId == character.CharacterID)
                        .ToListAsync();

                    foreach (var roll in rolls)
                    {
                        roll.CharacterId = null;
                    }

                    // Затем удаляем персонажа (без Include)
                    var characterToDelete = await context.Characters
                        .FirstOrDefaultAsync(c => c.CharacterID == character.CharacterID);

                    if (characterToDelete != null)
                    {
                        context.Characters.Remove(characterToDelete);
                        await context.SaveChangesAsync();

                        // Обновляем UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CharacterSheets.Remove(character);
                        });

                        MessageBox.Show($"Персонаж '{character.Name}' успешно удален");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка удаления персонажа");
                MessageBox.Show($"Не удалось удалить персонажа: {ex.Message}");
            }
        }

        private void PrintCurrentCharacter()
        {
            if (CurrentCharacter == null || string.IsNullOrEmpty(CurrentCharacter.Name))
            {
                MessageBox.Show("Сначала выберите или создайте персонажа");
                return;
            }

            try
            {
                // Создаем временный PDF файл
                string tempDir = Path.Combine(Path.GetTempPath(), "NRI_Print");
                Directory.CreateDirectory(tempDir);
                string outputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.pdf");

                // Проверяем наличие шаблона
                string templatesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
                string templateName = $"{CurrentCharacter.System.Replace(" ", "_")}_CharacterSheet.pdf";
                string templatePath = Path.Combine(templatesDir, templateName);

                if (!File.Exists(templatePath))
                {
                    // Если шаблона нет, создаем стандартный PDF
                    CreateDefaultPdf(outputPath);
                }
                else
                {
                    // Заполняем PDF шаблон
                    if (!PdfCharacterSheet.FillPdfTemplate(CurrentCharacter, templatePath, outputPath))
                    {
                        MessageBox.Show("Ошибка при создании PDF");
                        return;
                    }
                }

                // Открываем окно предпросмотра вместо прямой печати
                var previewWindow = new PDFPreviewWindow(outputPath);
                previewWindow.Owner = Application.Current.MainWindow;
                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        private void CreateDefaultPdf(string outputPath)
        {
            try
            {
                // Создаем документ
                Document document = new Document(PageSize.A4, 30, 30, 30, 30);

                // Создаем writer для записи в файл
                PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));

                // Открываем документ для записи
                document.Open();

                // Шрифты
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font fontNormal = new Font(baseFont, 12);
                Font fontBold = new Font(baseFont, 14, Font.BOLD);

                // Добавляем изображение, если оно есть
                if (!string.IsNullOrEmpty(CurrentCharacter.ImagePath) && File.Exists(CurrentCharacter.ImagePath))
                {
                    try
                    {
                        iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(CurrentCharacter.ImagePath);
                        image.ScaleToFit(150f, 150f);
                        image.Alignment = Element.ALIGN_RIGHT;
                        document.Add(image);
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex, "Ошибка добавления изображения в PDF");
                    }
                }

                // Заголовок
                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph($"Персонаж: {CurrentCharacter.Name}", new Font(baseFont, 18, Font.BOLD));
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Основная информация
                document.Add(new iTextSharp.text.Paragraph($"Система: {CurrentCharacter.System}", fontNormal));
                document.Add(new iTextSharp.text.Paragraph($"Раса: {CurrentCharacter.Race}", fontNormal));
                document.Add(new iTextSharp.text.Paragraph($"Класс: {CurrentCharacter.Class}", fontNormal));
                document.Add(new iTextSharp.text.Paragraph($"Уровень: {CurrentCharacter.Level}", fontNormal));

                // Дополнительные текстовые поля
                AddSectionToPdf(document, "Предыстория", CurrentCharacter.Backstory, fontBold, fontNormal);
                AddSectionToPdf(document, "Внешность", CurrentCharacter.Appearance, fontBold, fontNormal);
                AddSectionToPdf(document, "Черты характера", CurrentCharacter.PersonalityTraits, fontBold, fontNormal);
                AddSectionToPdf(document, "Идеалы", CurrentCharacter.Ideals, fontBold, fontNormal);
                AddSectionToPdf(document, "Привязанности", CurrentCharacter.Bonds, fontBold, fontNormal);
                AddSectionToPdf(document, "Слабости", CurrentCharacter.Flaws, fontBold, fontNormal);
                AddSectionToPdf(document, "Заметки", CurrentCharacter.Notes, fontBold, fontNormal);

                // Закрываем документ
                document.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF: {ex.Message}");
                _logger?.Error(ex, "Ошибка создания PDF");
            }
        }

        // Вспомогательный метод для добавления секций
        private void AddSectionToPdf(Document document, string title, string content, Font titleFont, Font contentFont)
        {
            if (!string.IsNullOrEmpty(content))
            {
                document.Add(new iTextSharp.text.Paragraph(title, titleFont));
                document.Add(new iTextSharp.text.Paragraph(content, contentFont));
                document.Add(new iTextSharp.text.Paragraph(" ")); // Пустая строка
            }
        }

        public CharacterSheet SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter != value)
                {
                    _selectedCharacter = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        // Загружаем полные данные персонажа
                        LoadCharacterData(value);
                        ApplySystemTemplate();
                    }
                    else
                    {
                        CurrentCharacter = new CharacterSheet();
                        CurrentTemplate = new SystemTemplate();
                    }

                    LoadRollHistoryForCharacter();
                }
            }
        }

        private void LoadCharacterData(CharacterSheet character)
        {
            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    // Загружаем только основные данные без Include
                    var loadedCharacter = context.Characters
                        .AsNoTracking()
                        .FirstOrDefault(c => c.CharacterID == character.CharacterID);

                    if (loadedCharacter != null)
                    {
                        // Десериализация JSON-свойств
                        loadedCharacter.AttributesCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterAttributeItem>>(loadedCharacter.AttributesJson);
                        loadedCharacter.SkillsCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterSkillItem>>(loadedCharacter.SkillsJson);
                        loadedCharacter.InventoryItems = JsonConvert.DeserializeObject<ObservableCollection<InventoryItem>>(loadedCharacter.InventoryJson);

                        CurrentCharacter = loadedCharacter;
                        OnPropertyChanged(nameof(CurrentCharacter));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка загрузки данных персонажа");
                MessageBox.Show($"Ошибка загрузки данных персонажа: {ex.Message}");
            }
        }


        public ICommand EnterEditModeCommand => new RelayCommand(() =>
        {
            if (SelectedCharacter != null)
            {
                IsEditing = true;
            }
        });

        public async void LoadRollHistoryForCharacter()
        {
            if (SelectedCharacter == null) return;

            try
            {
                RollHistory.Clear();

                using (var context = new AppDbContext(_dbOptions))
                {
                    var rolls = await context.DiceRolls
                        .Where(r => r.CharacterId == SelectedCharacter.CharacterID)
                        .OrderByDescending(r => r.Timestamp)
                        .ToListAsync();

                    foreach (var roll in rolls)
                    {
                        RollHistory.Add(roll);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка загрузки истории бросков");
                MessageBox.Show($"Ошибка загрузки истории бросков: {ex.Message}");
            }
        }

        private bool TryPrintWithPdfTemplate()
        {
            try
            {
                // Проверяем наличие папки Templates
                string templatesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
                if (!Directory.Exists(templatesDir))
                {
                    Directory.CreateDirectory(templatesDir);
                    MessageBox.Show($"Папка шаблонов создана: {templatesDir}\nПоместите туда PDF шаблоны для систем.");
                    return false;
                }

                // Формируем путь к шаблону
                string templateName = $"{CurrentCharacter.System.Replace(" ", "_")}_CharacterSheet.pdf";
                string templatePath = Path.Combine(templatesDir, templateName);

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show($"Шаблон {templateName} не найден в папке Templates");
                    return false;
                }

                // Создаем временный файл
                string tempDir = Path.Combine(Path.GetTempPath(), "NRI_Print");
                Directory.CreateDirectory(tempDir);
                string outputPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.pdf");

                // Заполняем шаблон
                if (!PdfCharacterSheet.FillPdfTemplate(CurrentCharacter, templatePath, outputPath))
                {
                    MessageBox.Show("Ошибка при заполнении шаблона");
                    return false;
                }

                // Открываем PDF для просмотра
                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}");
                return false;
            }
        }

        private void PrintPdfDocument(string pdfPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}");
            }
        }
        private void PrintDefaultCharacterSheet()
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var doc = CreateCharacterDocument();
                printDialog.PrintDocument(
                    ((IDocumentPaginatorSource)doc).DocumentPaginator,
                    $"Персонаж: {CurrentCharacter.Name}");
            }
        }

        private FlowDocument CreateCharacterDocument()
        {
            var doc = new FlowDocument
            {
                Background = Brushes.White,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Arial"),
                PagePadding = new Thickness(50),
                ColumnGap = 0,
                ColumnWidth = double.PositiveInfinity
            };

            // Заголовок
            var header = new System.Windows.Documents.Paragraph(new Run($"Персонаж: {CurrentCharacter.Name}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            header.Inlines.Add(new LineBreak());
            doc.Blocks.Add(header);

            // Основная информация в таблице
            var infoTable = new Table();
            infoTable.CellSpacing = 5;
            infoTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            infoTable.Columns.Add(new TableColumn { Width = GridLength.Auto });

            var rowGroup = new TableRowGroup();
            infoTable.RowGroups.Add(rowGroup);

            AddTableRow(rowGroup, "Система:", CurrentCharacter.System);
            AddTableRow(rowGroup, "Раса:", CurrentCharacter.Race);
            AddTableRow(rowGroup, "Класс:", CurrentCharacter.Class);
            AddTableRow(rowGroup, "Уровень:", CurrentCharacter.Level.ToString());

            doc.Blocks.Add(infoTable);
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run(new string('_', 50)))
            {
                TextAlignment = TextAlignment.Center
            });

            // Характеристики
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run("Характеристики:"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            foreach (var attribute in CurrentCharacter.AttributesCollection)
            {
                doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run($"{attribute.Name}: {attribute.Value}")));
            }

            // Навыки
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run("\nНавыки:"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            foreach (var skill in CurrentCharacter.SkillsCollection)
            {
                doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run($"{skill.Name}: {skill.Value}")));
            }

            // Инвентарь
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run("\nИнвентарь:"))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            foreach (var item in CurrentCharacter.InventoryItems)
            {
                doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run($"• {item.Name} x{item.Quantity}")));
            }

            // Примечания
            if (!string.IsNullOrEmpty(CurrentCharacter.Notes))
            {
                doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run("\nПримечания:"))
                {
                    FontWeight = FontWeights.Bold,
                    FontSize = 14
                });
                doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run(CurrentCharacter.Notes)));
            }

            return doc;
        }

        public SystemTemplate CurrentTemplate
        {
            get => _currentTemplate;
            set
            {
                _currentTemplate = value;
                OnPropertyChanged();
            }
        }

        private void AddTableRow(TableRowGroup rowGroup, string header, string value)
        {
            var row = new TableRow();

            var headerCell = new TableCell(new System.Windows.Documents.Paragraph(new Run(header))
            {
                FontWeight = FontWeights.Bold
            });

            var valueCell = new TableCell(new System.Windows.Documents.Paragraph(new Run(value)));

            row.Cells.Add(headerCell);
            row.Cells.Add(valueCell);
            rowGroup.Rows.Add(row);
        }


        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                LoadUserCharacters();
            }
        }

        private async Task LoadUserCharacters()
        {
            if (CurrentUser == null) return;

            try
            {
                CharacterSheets.Clear();

                using (var context = new AppDbContext(_dbOptions))
                {
                    var characters = await context.Characters
                        .Where(c => c.UserId == CurrentUser.Id)
                        .AsNoTracking()  // Добавлено для оптимизации производительности
                        .ToListAsync();

                    foreach (var character in characters)
                    {
                        CharacterSheets.Add(character);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка загрузки персонажей");
                MessageBox.Show($"Ошибка загрузки персонажей: {ex.Message}");
            }
        }

        private void AddAttribute()
        {
            CurrentCharacter.AttributesCollection.Add(new CharacterAttributeItem
            {
                Name = "Новый атрибут",
                Value = "0",
                CharacterId = CurrentCharacter.CharacterID
            });
            OnPropertyChanged(nameof(CurrentCharacter));
        }

        private void AddSkill()
        {
            if (CurrentCharacter == null) return;

            var newSkill = new CharacterSkillItem
            {
                Name = "Новый навык",
                Value = "0",
                IsProficient = false,
                CharacterId = CurrentCharacter.CharacterID
            };

            CurrentCharacter.SkillsCollection.Add(newSkill);
            CurrentCharacter.UpdateSkillsJson();
        }

        private void AddInventoryItem()
        {
            if (CurrentCharacter == null) return;

            var newItem = new InventoryItem
            {
                Name = "Новый предмет",
                Quantity = 1,
                CharacterId = CurrentCharacter.CharacterID,
                Description = "",
                IsEquipped = false
            };

            CurrentCharacter.InventoryItems.Add(newItem);
            CurrentCharacter.UpdateInventoryJson(); // Explicit update
            OnPropertyChanged(nameof(CurrentCharacter.InventoryItems));
        }

        private void RemoveInventoryItem(InventoryItem item)
        {
            if (item != null && CurrentCharacter?.InventoryItems.Contains(item) == true)
            {
                CurrentCharacter.InventoryItems.Remove(item);
                CurrentCharacter.UpdateInventoryJson(); // Explicitly update JSON
                OnPropertyChanged(nameof(CurrentCharacter.InventoryItems));
            }
        }


        private void RefreshInventory()
        {
            OnPropertyChanged(nameof(CurrentCharacter));
            OnPropertyChanged(nameof(CurrentCharacter.InventoryItems));

            // Принудительное обновление коллекции
            var temp = CurrentCharacter.InventoryItems.ToList();
            CurrentCharacter.InventoryItems.Clear();
            foreach (var item in temp)
            {
                CurrentCharacter.InventoryItems.Add(item);
            }
        }

        // Добавляем команду сброса навыков
        public ICommand ResetSkillsCommand => new RelayCommand(ResetSkillsToTemplate);

        private void ResetSkillsToTemplate()
        {
            if (CurrentCharacter == null || CurrentTemplate == null) return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите сбросить навыки к значениям шаблона? Все изменения будут потеряны.",
                "Подтверждение сброса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Сохраняем пользовательские навыки, которых нет в шаблоне
                var customSkills = CurrentCharacter.SkillsCollection
                    .Where(s => !CurrentTemplate.Tabs
                        .SelectMany(t => t.Skills ?? Enumerable.Empty<CharacterSkillItem>())
                        .Any(ts => ts.Name == s.Name))
                    .ToList();

                // Очищаем и заполняем заново
                CurrentCharacter.SkillsCollection.Clear();

                foreach (var tab in CurrentTemplate.Tabs)
                {
                    foreach (var skill in tab.Skills ?? Enumerable.Empty<CharacterSkillItem>())
                    {
                        CurrentCharacter.SkillsCollection.Add(new CharacterSkillItem
                        {
                            Name = skill.Name,
                            Value = skill.Value,
                            IsProficient = skill.IsProficient,
                            CharacterId = CurrentCharacter.CharacterID
                        });
                    }
                }

                // Добавляем обратно пользовательские навыки
                foreach (var skill in customSkills)
                {
                    CurrentCharacter.SkillsCollection.Add(skill);
                }

                CurrentCharacter.UpdateSkillsJson();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка сброса навыков");
                MessageBox.Show($"Ошибка сброса навыков: {ex.Message}");
            }
        }

        private void RemoveAttribute(CharacterAttributeItem attribute)
        {
            CurrentCharacter.AttributesCollection.Remove(attribute);
            OnPropertyChanged(nameof(CurrentCharacter));
        }

        private void RemoveSkill(CharacterSkillItem skill)
        {
            if (skill != null && CurrentCharacter?.SkillsCollection.Contains(skill) == true)
            {
                CurrentCharacter.SkillsCollection.Remove(skill);
                CurrentCharacter.UpdateSkillsJson();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
