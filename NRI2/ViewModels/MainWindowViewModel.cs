using CommonServiceLocator;
using DnsClient.Internal;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.API;
using NRI.Classes;
using NRI.Controls;
using NRI.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace NRI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ApiClient _apiClient;
        private readonly Timer _refreshTimer;
        private bool _showAllPanels = true;
        private User _currentUser;
        private List<string> _userRoles = new List<string>();
        private string _highestRole;

        private IServiceProvider _serviceProvider;
        public bool IsAuthenticated => CurrentUser?.IsTokenValid ?? false;
        private string _apiBaseUrl = "http://localhost:5000";
        public ObservableCollection<UserStatusDto> OnlineUsers { get; } = new ObservableCollection<UserStatusDto>();
        public MainWindowViewModel(ApiClient apiClient, ILogger<MainWindowViewModel> logger)
        {
            _apiClient = apiClient;
            _logger = logger;

            // Загружаем данные сразу
            LoadOnlineUsers().ConfigureAwait(false);

            // Обновляем каждые 30 секунд
            _refreshTimer = new Timer(_ => LoadOnlineUsers().Wait(),
                null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private async Task LoadOnlineUsers()
        {
            try
            {
                var users = await _apiClient.GetAsync<List<UserStatusDto>>("api/useractivity/active");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnlineUsers.Clear();
                    foreach (var user in users)
                    {
                        OnlineUsers.Add(user);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading online users: {ex.Message}");
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser == value) return;

                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UserGreeting));
                OnPropertyChanged(nameof(IsAuthenticated));

                if (value != null)
                {
                    UserRoles = value.Roles?.ToList() ?? new List<string> { "Игрок" };
                }
            }
        }

        public string UserGreeting => IsAuthenticated
             ? $"Добро пожаловать, {CurrentUser?.Login ?? "Пользователь"}"
             : "Гость";

        private bool _showRolePanels = true;
        public bool ShowRolePanels
        {
            get => _showRolePanels;
            set
            {
                if (_showRolePanels != value)
                {
                    _showRolePanels = value;
                    OnPropertyChanged();
                }
            }
        }

        private RelayCommand _togglePanelsCommand;
        public RelayCommand TogglePanelsCommand =>
            _togglePanelsCommand ??= new RelayCommand(ToggleRolePanels);

        private object _currentContent;
        public object CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                _logger?.LogError("ServiceProvider is null in Initialize");
                return;
            }

            _serviceProvider = serviceProvider;
            UpdateContentBasedOnRole();
        }


        private void ToggleRolePanels()
        {
            try
            {
                ShowRolePanels = !ShowRolePanels;
                UpdateContentBasedOnRole();
                _logger?.LogInformation($"Переключение панелей ролей. Новое состояние: {ShowRolePanels}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при переключении панелей ролей");
            }
        }

        public void UpdateContentBasedOnRole()
        {
            _logger.LogInformation($"UpdateContent: ShowRolePanel={ShowRolePanels}, Roles={string.Join(", ", UserRoles)}");
            if (_serviceProvider == null)
            {
                _logger?.LogWarning("ServiceProvider is null in UpdateContentBasedOnRole");
                return;
            }

            if (!ShowRolePanels)
            {
                CurrentContent = _serviceProvider.GetRequiredService<BaseWindowControl>();
                _logger?.LogInformation($"Показываем базовую панель. AdminWindow не загружен.");
                return;
            }

            _logger?.LogInformation($"Текущая роль: {HighestRole}");

            switch (HighestRole)
            {
                case "Администратор":
                    var adminControl = _serviceProvider.GetRequiredService<AdminWindowControl>();
                    CurrentContent = adminControl;
                    _logger?.LogInformation("Админ-панель загружена");
                    break;
                case "Организатор":
                    CurrentContent = _serviceProvider.GetRequiredService<OrganizerWindowControl>();
                    break;
                default:
                    CurrentContent = _serviceProvider.GetRequiredService<PlayerWindowControl>();
                    break;
            }
        }

        public void SetUserRoles(List<string> roles)
        {
            if (roles == null)
            {
                _logger.LogWarning("Получен null вместо списка ролей");
                roles = new List<string>();
            }

            // Фильтрация и проверка ролей
            roles = roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct()
                .ToList();

            if (!roles.Any())
            {
                _logger.LogWarning("Получен пустой список ролей после фильтрации");
                roles = new List<string> { "Игрок" };
            }

            _userRoles = roles;
            HighestRole = DetermineHighestRole(roles);

            _logger.LogInformation($"Установлены роли: {string.Join(", ", _userRoles)}");
            _logger.LogInformation($"Определена наивысшая роль: {HighestRole}");

            OnPropertyChanged(nameof(UserRoles));
            OnPropertyChanged(nameof(HighestRole));
            UpdateContentBasedOnRole();
        }
        public List<string> UserRoles
        {
            get => _userRoles;
            set
            {
                _logger.LogInformation($"Установка ролей: {string.Join(", ", value)}");
                _userRoles = value ?? new List<string> { "Игрок" };
                HighestRole = DetermineHighestRole(_userRoles);
                OnPropertyChanged();
            }
        }

        public string HighestRole
        {
            get => _highestRole;
            private set
            {
                _highestRole = value;
                OnPropertyChanged();
            }
        }

        public void SetUserFromClaims(ClaimsPrincipal principal)
        {
            var roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!roles.Any()) roles.Add("Игрок");

            CurrentUser = new User
            {
                Id = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                Login = principal.FindFirst(ClaimTypes.Name)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Roles = roles,
                Role = DetermineHighestRole(roles)
            };
        }

        private string DetermineHighestRole(List<string> roles)
        {
            if (roles == null || !roles.Any())
            {
                _logger.LogWarning("Определение роли: пустой список, возвращаем 'Игрок'");
                return "Игрок";
            }

            var roleHierarchy = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Администратор"] = 3,
                ["Организатор"] = 2,
                ["Игрок"] = 1
            };

            var highestRole = roles
                .Select(r => new {
                    Name = r,
                    Priority = roleHierarchy.TryGetValue(r, out var p) ? p : 0
                })
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

            return highestRole?.Priority > 0 ? highestRole.Name : "Игрок";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private string _selectedDiceType;
        public string SelectedDiceType
        {
            get => _selectedDiceType;
            set
            {
                if (_selectedDiceType != value)
                {
                    _selectedDiceType = value;
                    OnPropertyChanged();
                }
            }
        }
        private CharacterSheet _selectedCharacter;
        public CharacterSheet SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter != value)
                {
                    _selectedCharacter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedCharacter.System)); // Для обновления привязок к System
                }
            }
        }
        private ObservableCollection<DiceRolling> _recentRolls;
        public ObservableCollection<DiceRolling> RecentRolls
        {
            get => _recentRolls ?? (_recentRolls = new ObservableCollection<DiceRolling>());
            set
            {
                if (_recentRolls != value)
                {
                    _recentRolls = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _diceModifier;
        public int DiceModifier
        {
            get => _diceModifier;
            set
            {
                if (_diceModifier != value)
                {
                    _diceModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _diceCount = 1;
        public int DiceCount
        {
            get => _diceCount;
            set
            {
                if (_diceCount != value)
                {
                    _diceCount = value;
                    OnPropertyChanged();
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
