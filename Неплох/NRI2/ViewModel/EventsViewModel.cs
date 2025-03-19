using NRI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NRI.ViewModels
{
    public class EventsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Event> _events;
        public ObservableCollection<Event> Events
        {
            get { return _events; }
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        public EventsViewModel()
        {
            Events = new ObservableCollection<Event>(); // Инициализация коллекции
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}