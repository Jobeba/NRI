using NRI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NRI.ViewModels
{
    public class EventsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Events> _events;
        public ObservableCollection<Events> Events
        {
            get => _events;
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}