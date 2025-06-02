using GalaSoft.MvvmLight.Command;
using NRI.Classes;
using NRI.Services;
using NRI.Shared;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static NRI.Classes.User;


namespace NRI.ViewModels
{
    public class AdminViewModel : BaseViewModel, IDisposable
    {
        private readonly IUserService _userService;
        private readonly IGameSystemService _gameSystemService;
        private readonly DispatcherTimer _refreshTimer;
        private readonly IUserActivityService _activityService;
        private bool _isLoading;
        private readonly Timer _statsTimer;
        private readonly TimeSpan _statsUpdateInterval = TimeSpan.FromMinutes(1);

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<GameSystem> GameSystems { get; } = new ObservableCollection<GameSystem>();
        public ObservableCollection<UserStatusDto> OnlineUsers { get; set; }

        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand AddGameSystemCommand { get; }
        public ICommand DeleteGameSystemCommand { get; }
        public ICommand ImportTemplatesCommand { get; }

        public RelayCommand RefreshCommand { get; }
        public int UsersCount => Users.Count;

        public AdminViewModel(
                   IUserService userService,
                   IGameSystemService gameSystemService,
                   IUserActivityService activityService)
        {
            _userService = userService;
            _gameSystemService = gameSystemService;
            _activityService = activityService;

            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            AddUserCommand = new RelayCommand(AddUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);
            AddGameSystemCommand = new RelayCommand(AddGameSystem);
            DeleteGameSystemCommand = new RelayCommand(DeleteGameSystem);
            ImportTemplatesCommand = new RelayCommand(ImportTemplates);

            OnlineUsers = new ObservableCollection<UserStatusDto>();

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
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
                var users = await _userService.GetAllUserListAsync();
                var onlineUsers = await _activityService.GetActiveUsersAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        // Обновляем роли для пользователя
                        user.SetRolesFromCollection();
                        Users.Add(user);
                    }

                    OnlineUsers.Clear();
                    foreach (var user in onlineUsers)
                    {
                        OnlineUsers.Add(user);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
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
