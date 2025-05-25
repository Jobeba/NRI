using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NRI.DiceRoll
{
    public class CharacterAttributeItem : INotifyPropertyChanged
    {
        private string _name;
        private string _value;

        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        [NotMapped]
        public int Quantity { get; set; }

        [NotMapped]
        public bool IsEquipped { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CharacterSkillItem : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private bool _isProficient;

        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsProficient
        {
            get => _isProficient;
            set
            {
                if (_isProficient != value)
                {
                    _isProficient = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InventoryItem : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private int _quantity;
        private bool _isEquipped;

        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description =
                        value;
                    OnPropertyChanged();
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEquipped
        {
            get => _isEquipped;
            set
            {
                if (_isEquipped != value)
                {
                    _isEquipped = value;
                    OnPropertyChanged();
                }
            }
        }

        [NotMapped]
        public string Value => $"{Name} (x{Quantity})";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
