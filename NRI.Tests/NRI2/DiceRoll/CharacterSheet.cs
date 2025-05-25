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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

            // Десериализуем данные
            _attributesCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterAttributeItem>>(AttributesJson ?? "[]");
            _skillsCollection = JsonConvert.DeserializeObject<ObservableCollection<CharacterSkillItem>>(SkillsJson ?? "[]");
            _inventoryItems = JsonConvert.DeserializeObject<ObservableCollection<InventoryItem>>(InventoryJson ?? "[]");

            // Подписываемся на события новых коллекций
            SubscribeToEvents();

            OnPropertyChanged(nameof(AttributesCollection));
            OnPropertyChanged(nameof(SkillsCollection));
            OnPropertyChanged(nameof(InventoryItems));
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

        // Добавьте метод для инициализации из шаблона
        public void InitializeFromTemplate(SystemTemplate template)
        {
            if (template == null) return;

            // Очищаем только если коллекции пусты (новый персонаж)
            if (AttributesCollection.Count == 0)
            {
                AttributesCollection.Clear();
                foreach (var tab in template.Tabs)
                {
                    foreach (var group in tab.AttributeGroups ?? Enumerable.Empty<AttributeGroup>())
                    {
                        foreach (var attr in group.Attributes)
                        {
                            AttributesCollection.Add(new CharacterAttributeItem
                            {
                                Name = attr.Name,
                                Value = attr.Value,
                                CharacterId = this.CharacterID
                            });
                        }
                    }
                }
            }

            if (SkillsCollection.Count == 0)
            {
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
                            CharacterId = this.CharacterID
                        });
                    }
                }
            }

            UpdateAttributesJson();
            UpdateSkillsJson();
        }
        public string Name { get; set; }
        public string System { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public string ImagePath { get; set; }
        public string PlayerName { get; set; }
        public string Background { get; set; }
        public int? ExperiencePoints { get; set; }
        public string Alignment { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;


                [Column(TypeName = "nvarchar(MAX)")]
                public string AdditionalFields
                {
                    get => _additionalFieldsJson;
                    set
                    {
                        _additionalFieldsJson = value;
                        try
                        {
                            if (!string.IsNullOrEmpty(value))
                            {
                                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
                                _additionalFields.Clear();
                                foreach (var item in dict)
                                {
                                    _additionalFields.Add(item.Key, item.Value);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.Error(ex, "Error deserializing AdditionalFields");
                        }
                    }
                }

                private string _additionalFieldsJson;

                // Аналогично для других полей:
                [Column(TypeName = "nvarchar(MAX)")]
                public string Notes { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string Appearance { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string Backstory { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string PersonalityTraits { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string Ideals { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string Bonds { get; set; }

                [Column(TypeName = "nvarchar(MAX)")]
                public string Flaws { get; set; }

        [NotMapped]
        public ObservableCollection<DiceRolling> DiceRolls { get; set; } = new ObservableCollection<DiceRolling>();

        public CharacterSheet()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _additionalFields = new ObservableDictionary();

            // Инициализация коллекций с пустыми значениями и подписка на события
            _attributesCollection = new ObservableCollection<CharacterAttributeItem>();
            _skillsCollection = new ObservableCollection<CharacterSkillItem>();
            _inventoryItems = new ObservableCollection<InventoryItem>();

            // Подписка на события изменения коллекций
            SubscribeToEvents();

            // Инициализация JSON полей из коллекций
            UpdateAllJsonProperties();
        }
        private void SubscribeToEvents()
        {
            _attributesCollection.CollectionChanged += (s, e) => UpdateAttributesJson();
            foreach (var item in _attributesCollection)
                item.PropertyChanged += HandleAttributePropertyChanged;

            _skillsCollection.CollectionChanged += (s, e) => UpdateSkillsJson();
            foreach (var item in _skillsCollection)
                item.PropertyChanged += HandleSkillPropertyChanged;

            _inventoryItems.CollectionChanged += (s, e) => UpdateInventoryJson();
        }

        public void UpdateAllJsonProperties()
        {
            UpdateAttributesJson();
            UpdateSkillsJson();
            UpdateInventoryJson();
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
