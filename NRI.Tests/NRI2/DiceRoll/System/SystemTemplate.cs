using NRI.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DiceRoll
{
    public class SystemTemplate
    {
        public SystemTemplate()
        {
            Tabs = new ObservableCollection<SystemTab>();
            AttributeGroups = new ObservableCollection<AttributeGroup>();
            Skills = new ObservableCollection<CharacterSkillItem>();
            Inventory = new ObservableCollection<InventoryItem>();
            Races = new ObservableCollection<string>();
            Classes = new ObservableCollection<string>();
            AdditionalFields = new ObservableDictionary();
        }

        public string SystemName { get; set; }
        public ObservableCollection<SystemTab> Tabs { get; set; } = new ObservableCollection<SystemTab>();
        public ObservableCollection<AttributeGroup> AttributeGroups { get; set; } = new ObservableCollection<AttributeGroup>();
        public ObservableCollection<CharacterSkillItem> Skills { get; set; } = new ObservableCollection<CharacterSkillItem>();
        public ObservableCollection<InventoryItem> Inventory { get; set; } = new ObservableCollection<InventoryItem>();
        public ObservableCollection<string> Races { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Classes { get; set; } = new ObservableCollection<string>();
        public ObservableDictionary AdditionalFields { get; set; } = new ObservableDictionary();
        public string TemplateImagePath { get; set; }
    }

    public class AttributeGroup : INotifyPropertyChanged
    {
        private string _groupName;
        private int _columns;

        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; OnPropertyChanged(); }
        }

        public int Columns
        {
            get => _columns;
            set { _columns = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CharacterAttributeItem> Attributes { get; set; } = new ObservableCollection<CharacterAttributeItem>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}


