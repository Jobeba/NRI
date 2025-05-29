using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace NRI.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private string _username;
        private bool _isOnline;
        private DateTime? _lastActivity;
        private readonly DispatcherTimer _statusTimer;

        // Правильное объявление события PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsOnline
        {
            get => _isOnline;
            private set
            {
                if (_isOnline != value)
                {
                    _isOnline = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? LastActivity
        {
            get => _lastActivity;
            set
            {
                if (_lastActivity != value)
                {
                    _lastActivity = value;
                    OnPropertyChanged();
                    UpdateOnlineStatus(); // Обновляем статус при изменении времени активности
                }
            }
        }

        public UserViewModel()
        {
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _statusTimer.Tick += (s, e) => UpdateOnlineStatus();
            _statusTimer.Start();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateOnlineStatus()
        {
            IsOnline = LastActivity.HasValue &&
                     (DateTime.UtcNow - LastActivity.Value) <= TimeSpan.FromMinutes(5);
        }
    }
}
