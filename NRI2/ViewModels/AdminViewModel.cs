using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using NRI.Classes;
using NRI.Extensions;
using NRI.Models;
using NRI.Services;
using static NRI.Classes.User;

namespace NRI.ViewModels
{
    public class AdminViewModel : BaseViewModel, IDisposable
    {
        private readonly IUserService _userService;
        private readonly IGameSystemService _gameSystemService;
        private readonly DispatcherTimer _refreshTimer;
        private bool _isLoading;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<GameSystem> GameSystems { get; } = new ObservableCollection<GameSystem>();
        public ObservableCollection<User> OnlineUsers { get; } = new ObservableCollection<User>();

        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand AddGameSystemCommand { get; }
        public ICommand DeleteGameSystemCommand { get; }
        public ICommand ImportTemplatesCommand { get; }

        public int UsersCount => Users.Count;

        public AdminViewModel(IUserService userService, IGameSystemService gameSystemService)
        {
            _userService = userService;
            _gameSystemService = gameSystemService;

            AddUserCommand = new RelayCommand(AddUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            AddGameSystemCommand = new RelayCommand(AddGameSystem);
            DeleteGameSystemCommand = new RelayCommand(DeleteGameSystem);
            ImportTemplatesCommand = new RelayCommand(ImportTemplates);


            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30) // Обновляем статус каждые 30 секунд
            };
            _refreshTimer.Tick += async (s, e) => await LoadDataAsync();
            _refreshTimer.Start();

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                Users.Clear();
                var users = (await _userService.GetAllUserListAsync()).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Dispose()
        {
            _refreshTimer.Stop();
        }

        private async void AddUser()
        {
            try
            {
                var newUser = new User
                {
                    Login = "newuser",
                    Email = "new@example.com",
                    RegistrationDate = DateTime.Now
                };

                var result = await _userService.CreateUserAsync(newUser);
                if (result) Users.Add(newUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteUser()
        {
            try
            {
                if (Users.LastOrDefault() is User selectedUser)
                {
                    var result = await _userService.DeleteUserAsync(selectedUser.Id);
                    if (result) Users.Remove(selectedUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddGameSystem()
        {
            try
            {
                var newSystem = new GameSystem
                {
                    Name = "Новая система",
                    Version = "1.0",
                    Status = "Активна"
                };

                var result = await _gameSystemService.AddGameSystemAsync(newSystem);
                if (result) GameSystems.Add(newSystem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении системы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteGameSystem()
        {
            try
            {
                if (GameSystems.LastOrDefault() is GameSystem selectedSystem)
                {
                    var result = await _gameSystemService.DeleteGameSystemAsync(selectedSystem.Id);
                    if (result) GameSystems.Remove(selectedSystem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении системы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportTemplates()
        {
            MessageBox.Show("Функция импорта шаблонов", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void ShowUsersSection()
        {

        }
    }
}
