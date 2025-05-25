using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRI.DiceRoll
{
    // LanguageProficiency.cs
    public class LanguageProficiency : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }

        private string _language;
        private bool _canSpeak;
        private bool _canRead;
        private bool _canWrite;

        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }

        public bool CanSpeak
        {
            get => _canSpeak;
            set { _canSpeak = value; OnPropertyChanged(); }
        }

        public bool CanRead
        {
            get => _canRead;
            set { _canRead = value; OnPropertyChanged(); }
        }

        public bool CanWrite
        {
            get => _canWrite;
            set { _canWrite = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
