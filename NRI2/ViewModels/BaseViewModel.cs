using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NRI.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected RelayCommand CreateCommand(Action execute, Func<bool> canExecute = null)
        {
            return new RelayCommand(execute, canExecute);
        }

        protected RelayCommand<T> CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
        {
            return new RelayCommand<T>(execute, canExecute);
        }
    }
}
