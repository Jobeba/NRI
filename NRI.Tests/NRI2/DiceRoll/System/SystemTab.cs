using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NRI.Classes;

namespace NRI.DiceRoll
{
    public class SystemTab : INotifyPropertyChanged
    {
        private string _header;
        private ObservableCollection<AttributeGroup> _attributeGroups;
        private ObservableCollection<CharacterSkillItem> _skills;
        private ObservableCollection<InventoryItem> _inventory;
        private ObservableDictionary _additionalFields = new ObservableDictionary();

        private string _notes;

        public string Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AttributeGroup> AttributeGroups
        {
            get => _attributeGroups ??= new ObservableCollection<AttributeGroup>();
            set { _attributeGroups = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CharacterSkillItem> Skills
        {
            get => _skills ??= new ObservableCollection<CharacterSkillItem>();
            set { _skills = value; OnPropertyChanged(); }
        }

        public ObservableCollection<InventoryItem> Inventory
        {
            get => _inventory ??= new ObservableCollection<InventoryItem>();
            set { _inventory = value; OnPropertyChanged(); }
        }
        public ObservableDictionary AdditionalFields
        {
            get => _additionalFields;
            set
            {
                _additionalFields = value ?? new ObservableDictionary();
                OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
