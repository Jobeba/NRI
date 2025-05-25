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
        public ICommand ExportCharacterCommand => new RelayCommand<CharacterSheet>(ExportCharacter);
        public ICommand ImportCharacterCommand => new RelayCommand(ImportCharacter);

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
        public ICommand AddAttributeCommand => new RelayCommand(AddAttribute);
        public ICommand ResetAttributesCommand => new RelayCommand(ResetAttributesToTemplate);
        public ICommand RemoveAttributeCommand => new RelayCommand<CharacterAttributeItem>(RemoveAttribute);
        public ICommand DeleteCharacterCommand => new RelayCommand<CharacterSheet>(async (character) =>
        {
            if (character == null) return;

            await DeleteCharacterAsync(character);
        });
        public ICommand ResetSkillsCommand => new RelayCommand(ResetSkillsToTemplate);
        public ICommand PrintCurrentCharacterCommand => new RelayCommand(PrintCurrentCharacter);
        public ICommand LoadImageCommand => new RelayCommand(LoadCharacterImage);
        public ICommand CancelEditCommand => new RelayCommand(CancelEdit);
        public ICommand AddInventoryItemCommand { get; }
        public ICommand RemoveInventoryItemCommand { get; }
        public ICommand RemoveSkillCommand => new RelayCommand<CharacterSkillItem>(RemoveSkill);

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
        private void ExportCharacter(CharacterSheet character)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Файлы персонажа (*.chr)|*.chr|Все файлы (*.*)|*.*",
                DefaultExt = ".chr"
            };

            if (dialog.ShowDialog() == true)
            {
                var data = JsonConvert.SerializeObject(character, Formatting.Indented);
                File.WriteAllText(dialog.FileName, data);
            }
        }

        private void ImportCharacter()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Файлы персонажа (*.chr)|*.chr|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var data = File.ReadAllText(dialog.FileName);
                    var character = JsonConvert.DeserializeObject<CharacterSheet>(data);
                    CharacterSheets.Add(character);
                    CurrentCharacter = character;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка импорта: {ex.Message}");
                }
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
                    if (!await context.Database.CanConnectAsync())
                    {
                        _logger?.Error("Не удалось подключиться к базе данных");
                        MessageBox.Show("Ошибка подключения к базе данных");
                        return;
                    }

                    bool isNew = CurrentCharacter.CharacterID == 0;

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
                            context.Entry(existing).CurrentValues.SetValues(CurrentCharacter);
                        }
                    }

                    CurrentCharacter.LastModified = DateTime.Now;
                    CurrentCharacter.SerializeCollections();

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

        private async void RollDice()
        {
            if (string.IsNullOrEmpty(DiceType))
            {
                MessageBox.Show("Выберите тип кубика");
                return;
            }

            var results = new List<int>();
            int sum = 0;

            for (int i = 0; i < DiceCount; i++)
            {
                int value = DiceType == "FATE" ?
                    _random.Next(-1, 2) : 
                    _random.Next(1, GetMaxValue(DiceType) + 1);

                results.Add(value);
                sum += value;
            }

            var roll = new DiceRolling
            {
                DiceType = DiceType,
                DiceCount = DiceCount,
                Modifier = Modifier,
                Results = results,
                CharacterName = SelectedCharacter?.Name ?? "Без персонажа",
                UserId = _currentUser.Id,
                CharacterId = SelectedCharacter?.CharacterID
            };

            // Сохраняем бросок в БД
            await SaveRollToDatabase(roll);

            RollHistory.Insert(0, roll);
            SelectedRoll = roll;

            // Проверка на критические значения
            if (DiceType == "D20")
            {
                if (roll.IsCriticalSuccess)
                {
                    PlayCriticalSound(true);
                }
                else if (roll.IsCriticalFailure)
                {
                    PlayCriticalSound(false);
                }
            }
        }
        private async Task SaveRollToDatabase(DiceRolling roll)
        {
            try
            {
                using (var context = new AppDbContext(_dbOptions))
                {
                    context.DiceRolls.Add(roll);
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
            try
            {
                string soundName = isSuccess ? "critical_success" : "critical_fail";
                var uri = new Uri($"pack://application:,,,/NRI;component/Resources/{soundName}.wav", UriKind.Absolute);

                var resourceInfo = Application.GetResourceStream(uri);
                if (resourceInfo == null)
                {
                    Console.WriteLine($"Файл ресурса не найден: {uri}");
                    return;
                }

                using (var player = new SoundPlayer(resourceInfo.Stream))
                {
                    player.Play();
                }

                var message = isSuccess ?
                    "🎉 Критический успех! (20)" :
                    "💥 Критический провал! (1)";

                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(message, "Критический бросок",
                        MessageBoxButton.OK,
                        isSuccess ? MessageBoxImage.Exclamation : MessageBoxImage.Hand);
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");
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
                    { "Божество", "Нет" },
                    { "Особенности", "" },
                    { "Черты характера", "" },
                    { "Идеалы", "" },
                    { "Привязанности", "" },
                    { "Слабости", "" }
                }
            },

            new SystemTab
            {
                Header = "Языки",
                AdditionalFields = new ObservableDictionary
                {
                    { "Языки", "Общий" },
                    { "Дополнительные языки", "Выберите из списка" }
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

        public ObservableCollection<string> AvailableLanguages { get; } = new ObservableCollection<string>
        {
            "Общий", "Эльфийский", "Дварфский", "Орочий", "Гномий",
            "Полуросликов", "Тифлингов", "Драконов", "Язык глубин",
            "Язык элементалей", "Небесный", "Адский", "Абиссальный"
        };

        public ICommand AddLanguageCommand => new RelayCommand<string>(AddLanguage);
        public ICommand RemoveLanguageCommand => new RelayCommand<LanguageProficiency>(RemoveLanguage);

        private void AddLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language) || CurrentCharacter == null)
                return;

            if (!CurrentCharacter.Languages.Any(l => l.Language == language))
            {
                var newLang = new LanguageProficiency
                {
                    Language = language,
                    CanSpeak = true,
                    CanRead = true,
                    CanWrite = true,
                    CharacterId = CurrentCharacter.CharacterID
                };

                CurrentCharacter.Languages.Add(newLang);
                OnPropertyChanged(nameof(CurrentCharacter.Languages));
            }
        }

        private void RemoveLanguage(LanguageProficiency language)
        {
            if (language != null && CurrentCharacter.Languages.Contains(language))
            {
                CurrentCharacter.Languages.Remove(language);
                OnPropertyChanged(nameof(CurrentCharacter.Languages));
            }
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
            CurrentCharacter.UpdateSkillsJson(); // Обновляем JSON
            OnPropertyChanged(nameof(CurrentCharacter.SkillsCollection));
        }

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
                Document document = new Document();

                // Создаем writer для записи в файл
                PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));

                // Открываем документ для записи
                document.Open();

                // Шрифт для русского текста
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(baseFont, 12);
                Font boldFont = new Font(baseFont, 14, Font.BOLD);
                Font headerFont = new Font(baseFont, 16, Font.BOLD);

                // Заголовок
                document.Add(new iTextSharp.text.Paragraph($"Персонаж: {CurrentCharacter.Name}", boldFont)
                {
                    Alignment = Element.ALIGN_CENTER
                });

                // Основная информация
                document.Add(new iTextSharp.text.Paragraph($"Система: {CurrentCharacter.System}", font));
                document.Add(new iTextSharp.text.Paragraph($"Раса: {CurrentCharacter.Race}", font));
                document.Add(new iTextSharp.text.Paragraph($"Класс: {CurrentCharacter.Class}", font));
                document.Add(new iTextSharp.text.Paragraph($"Уровень: {CurrentCharacter.Level}", font));

                if (CurrentCharacter.System == "Call of Cthulhu")
                {
                    // Характеристики в таблице
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 100;
                    table.SpacingBefore = 10f;
                    table.SpacingAfter = 10f;

                    // Заголовок таблицы
                    PdfPCell headerCell = new PdfPCell(new Phrase("Характеристики", boldFont));
                    headerCell.Colspan = 2;
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.BackgroundColor = new BaseColor(200, 200, 200);
                    table.AddCell(headerCell);

                    // Добавляем характеристики
                    foreach (var attr in CurrentCharacter.AttributesCollection)
                    {
                        table.AddCell(new Phrase(attr.Name, font));
                        table.AddCell(new Phrase(attr.Value, font));
                    }

                    document.Add(table);
                }
                else
                {
                    // Стандартное отображение для других систем
                    document.Add(new iTextSharp.text.Paragraph("\nХарактеристики:", boldFont));
                    foreach (var attr in CurrentCharacter.AttributesCollection)
                    {
                        document.Add(new iTextSharp.text.Paragraph($"{attr.Name}: {attr.Value}", font));
                    }
                }

                // Характеристики
                document.Add(new iTextSharp.text.Paragraph("\nХарактеристики:", boldFont));
                foreach (var attr in CurrentCharacter.AttributesCollection)
                {
                    document.Add(new iTextSharp.text.Paragraph($"{attr.Name}: {attr.Value}", font));
                }

                // Навыки
                document.Add(new iTextSharp.text.Paragraph("\nНавыки:", boldFont));
                foreach (var skill in CurrentCharacter.SkillsCollection)
                {
                    document.Add(new iTextSharp.text.Paragraph($"{skill.Name}: {skill.Value}", font));
                }

                // Инвентарь
                document.Add(new iTextSharp.text.Paragraph("\nИнвентарь:", boldFont));
                foreach (var item in CurrentCharacter.InventoryItems)
                {
                    document.Add(new iTextSharp.text.Paragraph($"- {item.Name} x{item.Quantity}", font));
                }

                // Закрываем документ
                document.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании PDF: {ex.Message}");
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
            if (CurrentCharacter.AttributesCollection.Any(a =>
                string.IsNullOrWhiteSpace(a.Name)))
            {
                MessageBox.Show("Заполните имя существующего атрибута перед добавлением нового");
                return;
            }

            CurrentCharacter.AttributesCollection.Add(new CharacterAttributeItem
            {
                Name = "Новый атрибут",
                Value = "0"
            });
        }

        public ICommand AddSkillCommand => new RelayCommand(() =>
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
            OnPropertyChanged(nameof(CurrentCharacter.SkillsCollection));
        });

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
            OnPropertyChanged(nameof(CurrentCharacter.InventoryItems));
        }

        private void RemoveInventoryItem(InventoryItem item)
        {
            if (item != null && CurrentCharacter.InventoryItems.Contains(item))
            {
                CurrentCharacter.InventoryItems.Remove(item);
                RefreshInventory();
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

        private void ResetSkillsToTemplate()
        {
            if (CurrentCharacter == null || CurrentTemplate == null) return;

            var result = MessageBox.Show(
                "Сбросить навыки к шаблону системы? Все текущие навыки будут удалены.",
                "Подтверждение",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var tab in CurrentTemplate.Tabs)
                {
                    if (tab.Skills != null)
                    {
                        CurrentCharacter.SkillsCollection.Clear();
                        foreach (var skill in tab.Skills)
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
                }
            }
        }
        private void RemoveAttribute(CharacterAttributeItem attribute)
        {
            if (attribute != null && CurrentCharacter.AttributesCollection.Contains(attribute))
            {
                CurrentCharacter.AttributesCollection.Remove(attribute);
            }
        }
        private void ResetAttributesToTemplate()
        {
            if (CurrentCharacter == null || CurrentTemplate == null) return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите сбросить атрибуты к значениям шаблона? Все изменения будут потеряны.",
                "Подтверждение сброса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Сохраняем пользовательские атрибуты, которых нет в шаблоне
                var customAttributes = CurrentCharacter.AttributesCollection
                    .Where(a => !CurrentTemplate.Tabs
                        .SelectMany(t => t.AttributeGroups)
                        .SelectMany(g => g.Attributes)
                        .Any(ta => ta.Name == a.Name))
                    .ToList();

                // Очищаем и заполняем заново
                CurrentCharacter.AttributesCollection.Clear();

                foreach (var tab in CurrentTemplate.Tabs)
                {
                    foreach (var group in tab.AttributeGroups ?? Enumerable.Empty<AttributeGroup>())
                    {
                        foreach (var attribute in group.Attributes ?? Enumerable.Empty<CharacterAttributeItem>())
                        {
                            CurrentCharacter.AttributesCollection.Add(new CharacterAttributeItem
                            {
                                Name = attribute.Name,
                                Value = attribute.Value,
                                CharacterId = CurrentCharacter.CharacterID
                            });
                        }
                    }
                }

                // Добавляем обратно пользовательские атрибуты
                foreach (var attribute in customAttributes)
                {
                    CurrentCharacter.AttributesCollection.Add(attribute);
                }

                CurrentCharacter.UpdateAttributesJson();
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка сброса атрибутов");
                MessageBox.Show($"Ошибка сброса атрибутов: {ex.Message}");
            }
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
        // Внутри класса DiceRollerViewModel
        public ICommand ChangeFontSizeCommand => new RelayCommand<string>(fontSizeStr =>
        {
            if (CurrentCharacter == null || string.IsNullOrEmpty(fontSizeStr)) return;

            try
            {
                var fontSize = double.Parse(fontSizeStr);
                ApplyFormattingToSelection(TextElement.FontSizeProperty, fontSize);
            }
            catch (FormatException)
            {
                _logger?.Warn($"Некорректный размер шрифта: {fontSizeStr}");
            }
        });

        public ICommand ChangeFontColorCommand => new RelayCommand<string>(colorName =>
        {
            if (CurrentCharacter == null || string.IsNullOrEmpty(colorName)) return;

            try
            {
                var color = colorName switch
                {
                    "Черный" => Colors.Black,
                    "Красный" => Colors.Red,
                    "Зеленый" => Colors.Green,
                    "Синий" => Colors.Blue,
                    _ => Colors.Black
                };

                ApplyFormattingToSelection(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, $"Ошибка изменения цвета текста: {colorName}");
            }
        });

        public ICommand ChangeAlignmentCommand => new RelayCommand<string>(alignment =>
        {
            if (CurrentCharacter == null || string.IsNullOrEmpty(alignment)) return;

            try
            {
                var textAlignment = alignment switch
                {
                    "По левому краю" => TextAlignment.Left,
                    "По центру" => TextAlignment.Center,
                    "По правому краю" => TextAlignment.Right,
                    "По ширине" => TextAlignment.Justify,
                    _ => TextAlignment.Left
                };

                ApplyFormattingToSelection(System.Windows.Documents.Paragraph.TextAlignmentProperty, textAlignment);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, $"Ошибка изменения выравнивания: {alignment}");
            }
        });

        private void ApplyFormattingToSelection(DependencyProperty property, object value)
        {
            // Получаем RichTextBox из визуального дерева
            var richTextBox = FindVisualChild<RichTextBox>(Application.Current.MainWindow);
            if (richTextBox == null || !richTextBox.IsFocused) return;

            var selection = richTextBox.Selection;
            if (selection == null || selection.IsEmpty) return;

            // Применяем форматирование к выделенному тексту
            selection.ApplyPropertyValue(property, value);
        }

        // Вспомогательный метод для поиска элемента в визуальном дереве
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

    }
}
