using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NRI
{
    public partial class Autorizatsaya : Window
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<Autorizatsaya> _logger;

        public Autorizatsaya(IAuthService authService, INavigationService navigationService, ILogger<Autorizatsaya> logger)
        {
            InitializeComponent();
            DataContext = this; // Установка DataContext

            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Инициализация команды для кнопки регистрации
            RegistrationCommand = new RelayCommand(async () => await RegisterAsync());
        }

        public ICommand RegistrationCommand { get; }

        private async Task RegisterAsync()
        {
            try
            {
                _logger.LogInformation("Попытка регистрации пользователя: {0}", LoginBox.Text);

                if (!ValidateInput(out var errorMessage))
                {
                    await ShowMessageAsync(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var loginResult = await _authService.LoginAsync(LoginBox.Text, Passbox.Password);

                if (loginResult.IsSuccess)
                {
                    await HandleSuccessfulLogin(loginResult.User);
                }
                else
                {
                    await HandleFailedLogin(loginResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя: {0}", LoginBox.Text);
                await ShowMessageAsync("Произошла ошибка при регистрации. Подробности в логах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput(out string errorMessage)
        {
            if (string.IsNullOrEmpty(LoginBox.Text) || LoginBox.Text.Length < 3)
            {
                errorMessage = "Логин должен содержать минимум 3 символа";
                return false;
            }

            if (string.IsNullOrEmpty(Passbox.Password) || Passbox.Password.Length < 6)
            {
                errorMessage = "Пароль должен содержать минимум 6 символов";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private async Task HandleSuccessfulLogin(DataTable user)
        {
            try
            {
                var login = LoginBox.Text;

                await _authService.UpdateBlockStatusAsync(login);

                if (await _authService.IsUserBlockedAsync(login))
                {
                    await ShowMessageAsync("Прошло 30 дней, ваша учётная запись просрочена. Обратитесь к администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                    return;
                }

                if (await _authService.GetIncorrectAttemptsAsync(login) > 3)
                {
                    await ShowMessageAsync("Ваша учетная запись заблокирована. Обратитесь к администратору системы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Dispatcher.Invoke(() => Close()); // Закрытие окна в UI-потоке
                    return;
                }

                await _authService.ResetIncorrectAttemptsAsync(login);

                if (!await _authService.IsPasswordConfirmationRequiredAsync(login))
                {
                    await ShowMessageAsync("Замените пароль при первом входе", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    _navigationService.NavigateTo<ChangePassword>();
                }
                else
                {
                    await ShowMessageAsync("Вы успешно авторизовались", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    _navigationService.NavigateTo<MainWindow>();
                    Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка после успешной авторизации");
                await ShowMessageAsync("Ошибка при обработке данных пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task HandleFailedLogin(string errorMessage)
        {
            var login = LoginBox.Text;

            await _authService.IncrementIncorrectAttemptsAsync(login);

            if (await _authService.GetIncorrectAttemptsAsync(login) > 3)
            {
                await ShowMessageAsync("Вы заблокированы. Обратитесь к администратору", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
            }
            else
            {
                await ShowMessageAsync(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowMessageAsync(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            await Dispatcher.InvokeAsync(() => MessageBox.Show(message, title, button, image));
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new Registrasya();
            registrationWindow.Show();
            Close();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true); // По умолчанию команда всегда доступна
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}