using NRI.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace NRI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {

        private User _currentUser;
        private List<string> _userRoles = new List<string>();
        private string _highestRole;
        public bool IsAuthenticated => CurrentUser?.IsTokenValid ?? false;

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

        public void SetUserRoles(List<string> roles)
        {
            UserRoles = roles;
            System.Diagnostics.Debug.WriteLine($"Установлены роли: {string.Join(", ", UserRoles)}");
            System.Diagnostics.Debug.WriteLine($"Наивысшая роль: {HighestRole}");
        }

        public List<string> UserRoles
        {
            get => _userRoles;
            set
            {
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
            if (principal == null)
            {
                CurrentUser = null;
                return;
            }

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
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
            var roleHierarchy = new Dictionary<string, int>
        {
            { "Администратор", 3 },
            { "Организатор", 2 },
            { "Игрок", 1 }
        };

            return roles
                .OrderByDescending(r => roleHierarchy.ContainsKey(r) ? roleHierarchy[r] : 0)
                .FirstOrDefault() ?? "Игрок";
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
