using NRI.DiceRoll;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using NLog;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Web.UI;
using System.Collections.Specialized;
using System.Windows.Threading;
using NRI.Data;

namespace NRI.Classes
{
    public class CharacterSheet : INotifyPropertyChanged
    {
        private ILogger _logger;

        private ObservableDictionary _additionalFields = new ObservableDictionary();
        private ObservableCollection<CharacterAttributeItem> _attributesCollection;
        private ObservableCollection<CharacterSkillItem> _skillsCollection;
        private ObservableCollection<InventoryItem> _inventoryItems;

        [Key]
        [Column("CharacterID")]
        public int CharacterID { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [ForeignKey("User")]
        [Column("UserID")]
        public int UserId { get; set; }

        [Column("Attributes")]
        public string AttributesJson { get; set; }

        [Column("Skills")]
        public string SkillsJson { get; set; }

        [Column("Inventory")]
        public string InventoryJson { get; set; }
        private void HandleAttributesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateAttributesJson();
        }

        private void HandleAttributePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateAttributesJson();
        }

        private void HandleSkillsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSkillsJson();
        }

        private void HandleSkillPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSkillsJson();
        }

        private void HandleInventoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateInventoryJson();
        }

        [NotMapped]
        public ObservableCollection<CharacterAttributeItem> AttributesCollection
        {
            get
            {
                if (_attributesCollection == null)
                {
                    _attributesCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterAttributeItem>>(AttributesJson ?? "[]");
                    _attributesCollection.CollectionChanged += HandleAttributesCollectionChanged;

                    foreach (var item in _attributesCollection)
                    {
                        item.PropertyChanged += HandleAttributePropertyChanged;
                    }
                }
                return _attributesCollection;
            }
            set
            {
                if (_attributesCollection != null)
                {
                    _attributesCollection.CollectionChanged -= HandleAttributesCollectionChanged;
                    foreach (var item in _attributesCollection)
                    {
                        item.PropertyChanged -= HandleAttributePropertyChanged;
                    }
                }

                _attributesCollection = value;

                if (_attributesCollection != null)
                {
                    _attributesCollection.CollectionChanged += HandleAttributesCollectionChanged;
                    foreach (var item in _attributesCollection)
                    {
                        item.PropertyChanged += HandleAttributePropertyChanged;
                    }
                }

                UpdateAttributesJson();
                OnPropertyChanged();
            }
        }


        [NotMapped]
        public ObservableCollection<CharacterSkillItem> SkillsCollection
        {
            get
            {
                if (_skillsCollection == null)
                {
                    _skillsCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterSkillItem>>(SkillsJson ?? "[]");
                    _skillsCollection.CollectionChanged += HandleSkillsCollectionChanged;

                    foreach (var item in _skillsCollection)
                    {
                        item.PropertyChanged += HandleSkillPropertyChanged;
                    }
                }
                return _skillsCollection;
            }
            set
            {
                if (_skillsCollection != null)
                {
                    _skillsCollection.CollectionChanged -= HandleSkillsCollectionChanged;
                    foreach (var item in _skillsCollection)
                    {
                        item.PropertyChanged -= HandleSkillPropertyChanged;
                    }
                }

                _skillsCollection = value;

                if (_skillsCollection != null)
                {
                    _skillsCollection.CollectionChanged += HandleSkillsCollectionChanged;
                    foreach (var item in _skillsCollection)
                    {
                        item.PropertyChanged += HandleSkillPropertyChanged;
                    }
                }

                UpdateSkillsJson();
                OnPropertyChanged();
            }
        }
        [NotMapped]
        public ObservableCollection<InventoryItem> InventoryItems
        {
            get
            {
                if (_inventoryItems == null)
                {
                    _inventoryItems = JsonConvert.DeserializeObject<ObservableCollection<InventoryItem>>(InventoryJson ?? "[]");
                    _inventoryItems.CollectionChanged += HandleInventoryCollectionChanged;
                }
                return _inventoryItems;
            }
            set
            {
                if (_inventoryItems != null)
                {
                    _inventoryItems.CollectionChanged -= HandleInventoryCollectionChanged;
                }

                _inventoryItems = value;

                if (_inventoryItems != null)
                {
                    _inventoryItems.CollectionChanged += HandleInventoryCollectionChanged;
                }

                UpdateInventoryJson();
                OnPropertyChanged();
            }
        }
        // Методы для обновления JSON свойств
        public void UpdateAttributesJson()
        {
            _logger?.Debug("Updating AttributesJson");
            AttributesJson = JsonConvert.SerializeObject(_attributesCollection);
            OnPropertyChanged(nameof(AttributesCollection));
        }

        public void UpdateSkillsJson()
        {
            SkillsJson = JsonConvert.SerializeObject(_skillsCollection);
            OnPropertyChanged(nameof(SkillsCollection));
        }

        public void UpdateInventoryJson()
        {
            InventoryJson = JsonConvert.SerializeObject(_inventoryItems);
            OnPropertyChanged(nameof(InventoryItems));
        }

        public ObservableDictionary AdditionalFields
        {
            get => _additionalFields;
            set
            {
                _additionalFields = value;
                OnPropertyChanged();
            }
        }

        public void SerializeCollections()
        {
            UpdateAttributesJson();
            UpdateSkillsJson();
            UpdateInventoryJson();
        }

        public void DeserializeCollections()
        {
            // Временно отписываемся от всех событий
            UnsubscribeFromEvents();

            try
            {
                // Десериализуем данные с проверкой на null
                _attributesCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterAttributeItem>>(
                    AttributesJson ?? "[]") ?? new ObservableCollection<CharacterAttributeItem>();

                _skillsCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterSkillItem>>(
                    SkillsJson ?? "[]") ?? new ObservableCollection<CharacterSkillItem>();

                _inventoryItems = JsonConvert.DeserializeObject<ObservableCollection<InventoryItem>>(
                    InventoryJson ?? "[]") ?? new ObservableCollection<InventoryItem>();

                Languages = JsonConvert.DeserializeObject<ObservableCollection<LanguageProficiency>>(LanguagesJson ?? "[]");
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Ошибка десериализации коллекций");

                // Создаем пустые коллекции в случае ошибки
                _attributesCollection = new ObservableCollection<CharacterAttributeItem>();
                _skillsCollection = new ObservableCollection<CharacterSkillItem>();
                _inventoryItems = new ObservableCollection<InventoryItem>();
            }

            // Подписываемся на события новых коллекций
            SubscribeToEvents();

            OnPropertyChanged(nameof(AttributesCollection));
            OnPropertyChanged(nameof(SkillsCollection));
            OnPropertyChanged(nameof(InventoryItems));
        }
        public void UpdateLanguagesJson()
        {
            LanguagesJson = JsonConvert.SerializeObject(Languages);
        }

        private void UnsubscribeFromEvents()
        {
            if (_attributesCollection != null)
            {
                _attributesCollection.CollectionChanged -= HandleAttributesCollectionChanged;
                foreach (var item in _attributesCollection)
                    item.PropertyChanged -= HandleAttributePropertyChanged;
            }

            if (_skillsCollection != null)
            {
                _skillsCollection.CollectionChanged -= HandleSkillsCollectionChanged;
                foreach (var item in _skillsCollection)
                    item.PropertyChanged -= HandleSkillPropertyChanged;
            }

            if (_inventoryItems != null)
                _inventoryItems.CollectionChanged -= HandleInventoryCollectionChanged;
        }

        private void SubscribeToEvents()
        {
            if (_attributesCollection != null)
            {
                _attributesCollection.CollectionChanged += HandleAttributesCollectionChanged;
                foreach (var item in _attributesCollection)
                    item.PropertyChanged += HandleAttributePropertyChanged;
            }

            if (_skillsCollection != null)
            {
                _skillsCollection.CollectionChanged += HandleSkillsCollectionChanged;
                foreach (var item in _skillsCollection)
                    item.PropertyChanged += HandleSkillPropertyChanged;
            }

            if (_inventoryItems != null)
                _inventoryItems.CollectionChanged += HandleInventoryCollectionChanged;
        }

        [Column("AdditionalFields")]
        public string AdditionalFieldsJson
        {
            get
            {
                try
                {
                    var dict = _additionalFields.ToDictionary(x => x.Key, x => x.Value);
                    return JsonConvert.SerializeObject(dict, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Ошибка сериализации AdditionalFields");
                    return "{}";
                }
            }
            set
            {
                try
                {
                    _additionalFields.Clear();

                    if (string.IsNullOrWhiteSpace(value))
                        return;

                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(value)
                              ?? new Dictionary<string, string>();

                    foreach (var item in dict)
                    {
                        _additionalFields.Add(item.Key, item.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "Ошибка десериализации AdditionalFields");
                    _additionalFields.Clear();
                }
            }
        }
        // Добавьте метод для инициализации из шаблона
        public void InitializeFromTemplate(SystemTemplate template)
        {
            if (template == null) return;

            Languages.Clear();
            if (template.SystemName == "D&D 5e")
            {
                HasDarkvision = false; 

                // Добавляем стандартные языки
                Languages.Add(new LanguageProficiency { Language = "Общий", CanSpeak = true, CanRead = true, CanWrite = true });
            }

            // Инициализация атрибутов
            AttributesCollection.Clear();
            foreach (var tab in template.Tabs)
            {
                foreach (var group in tab.AttributeGroups ?? Enumerable.Empty<AttributeGroup>())
                {
                    foreach (var attr in group.Attributes ?? Enumerable.Empty<CharacterAttributeItem>())
                    {
                        AttributesCollection.Add(new CharacterAttributeItem
                        {
                            Name = attr.Name,
                            Value = attr.Value,
                            CharacterId = CharacterID
                        });
                    }
                }
            }

            // Инициализация навыков
            SkillsCollection.Clear();
            foreach (var tab in template.Tabs)
            {
                foreach (var skill in tab.Skills ?? Enumerable.Empty<CharacterSkillItem>())
                {
                    SkillsCollection.Add(new CharacterSkillItem
                    {
                        Name = skill.Name,
                        Value = skill.Value,
                        IsProficient = skill.IsProficient,
                        CharacterId = CharacterID
                    });
                }
            }

            // Инициализация инвентаря
            InventoryItems.Clear();
            foreach (var tab in template.Tabs)
            {
                foreach (var item in tab.Inventory ?? Enumerable.Empty<InventoryItem>())
                {
                    InventoryItems.Add(new InventoryItem
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        IsEquipped = item.IsEquipped,
                        CharacterId = CharacterID
                    });
                }
            }
        }

        public string LanguagesJson
        {
            get => JsonConvert.SerializeObject(Languages);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    Languages = JsonConvert.DeserializeObject<ObservableCollection<LanguageProficiency>>(value) ?? new();
                else
                    Languages.Clear();
            }
        }

        public string Name { get; set; }
        public string System { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public string Notes { get; set; }
        public string ImagePath { get; set; }
        public string PlayerName { get; set; }
        public string Background { get; set; }
        public int? ExperiencePoints { get; set; }
        public string Alignment { get; set; }
        public string Appearance { get; set; }
        public string Backstory { get; set; }
        public string PersonalityTraits { get; set; }
        public string Ideals { get; set; }
        public string Bonds { get; set; }
        public string Flaws { get; set; }

        private DispatcherTimer _autoSaveTimer;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;

        public bool HasDarkvision { get; set; }


        private ObservableCollection<LanguageProficiency> _languages = new();
        public ObservableCollection<LanguageProficiency> Languages
        {
            get => _languages;
            set
            {
                _languages = value;
                OnPropertyChanged();
            }
        }

        [NotMapped]
        public ObservableCollection<DiceRolling> DiceRolls { get; set; } = new ObservableCollection<DiceRolling>();

        public CharacterSheet()
        {
            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _autoSaveTimer.Tick += AutoSave;
            _autoSaveTimer.Start();

            _logger = LogManager.GetCurrentClassLogger();
            _additionalFields = new ObservableDictionary();
            SkillsCollection = new ObservableCollection<CharacterSkillItem>();
            // Инициализация коллекций
            _attributesCollection = new ObservableCollection<CharacterAttributeItem>();
            _skillsCollection = new ObservableCollection<CharacterSkillItem>();
            _inventoryItems = new ObservableCollection<InventoryItem>();

            Languages = new ObservableCollection<LanguageProficiency>();

            // Подписка на события изменения коллекций
            _attributesCollection.CollectionChanged += (s, e) => UpdateAttributesJson();
            _skillsCollection.CollectionChanged += (s, e) => UpdateSkillsJson();
            _inventoryItems.CollectionChanged += (s, e) => UpdateInventoryJson();
        }
        private async void AutoSave(object sender, EventArgs e)
        {
            if (CharacterID == 0) return;

            try
            {
                using (var context = new AppDbContext())
                {
                    var existing = await context.Characters.FindAsync(CharacterID);
                    if (existing != null)
                    {
                        this.SerializeCollections();
                        context.Entry(existing).CurrentValues.SetValues(this);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoSave error: {ex.Message}");
            }
        }

        public virtual User User { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateJsonProperties()
        {
            UpdateAttributesJson();
            UpdateSkillsJson();
            UpdateInventoryJson();
        }

        public class CharacterAttribute
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class CharacterSkill
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public bool IsProficient { get; set; }
        }
    }
}
