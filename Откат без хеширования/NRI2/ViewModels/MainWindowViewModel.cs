using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NRI.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Использование лямбда-выражения для команд
            EventsCommand = new RelayCommand(() => OpenEvents());
            PlayersCommand = new RelayCommand(() => OpenPlayers());
            // Добавьте другие команды
        }

        public ICommand EventsCommand { get; }
        public ICommand PlayersCommand { get; }
        // Добавьте другие команды

        private void OpenEvents()
        {
            var EventsWindow = _serviceProvider.GetRequiredService<Events>();
            EventsWindow.Show();
        }

        private void OpenPlayers()
        {
            var playerWindow = _serviceProvider.GetRequiredService<Player>();
            playerWindow.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}