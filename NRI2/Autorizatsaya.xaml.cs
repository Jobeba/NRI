using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Data.SqlClient;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using WpfAnimatedGif;
using System.Windows.Threading;
using System.ComponentModel;
using Dapper;
using NRI.Classes.Email;
using NRI.Helpers;
using static NRI.DiceRoll.DiceRollerViewModel;
using NRI.DiceRoll;
using NRI.ViewModels;
using NRI.Data;
using NRI.Services;
using NRI.Classes;
using NRI.Pages;
using NRI.DB;

namespace NRI
{
    public partial class Autorizatsaya : Window, INotifyPropertyChanged, IDisposable
    {
        private readonly ILogger<TwoFactorSetupDialog> _twoFactorLogger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEmailSenderService _emailSender;
        private readonly IUserRepository _userRepository;
        private readonly AnimationManager _animationManager = new AnimationManager(TimeSpan.FromMilliseconds(400));
        private bool _isWindowTransitionInProgress;
        private ScaleTransform _backgroundScale;
        private BlurEffect _backgroundBlur;
        private Border _mainBorder;
        private ScaleTransform _contentScale;
        private PackIcon _toggleIcon;
        private readonly TimeSpan _animationDuration = TimeSpan.FromMilliseconds(400);
        private ScaleTransform _scaleTransform;

        // WinAPI константы и функции
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]

        private static extern bool ReleaseCapture();
        private bool _isAnimating;
        private bool _isFullscreen = true;
        private Point _windowPosition;
        private Size _windowSize;
        private readonly IEmailTemplateService _emailTemplateService;
        private string _currentUserEmail;
        private string _enteredSecretKey;
        private int _passwordStrength;
        private string _currentUserLogin;
        private string _tempSecretKey;
        private readonly IConfigService _configService;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ILogger<Autorizatsaya> _logger;
        public void Dispose()
        {
            CleanupAnimations();
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);
            BackgroundImage.Source = null;
        }
        public ICommand LoginCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand VerifyTwoFactorCommand { get; }
        public ICommand ConfirmPasswordCommand { get; }
        private void HandleResize(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal && e.LeftButton == MouseButtonState.Pressed)
            {
                var border = (FrameworkElement)sender;
                var direction = border.Tag.ToString();
                var directionCode = GetDirectionCode(direction);
                ReleaseCapture();
                var windowHandle = new WindowInteropHelper(this).Handle;
                SendMessage(windowHandle, WM_NCLBUTTONDOWN, (IntPtr)directionCode, IntPtr.Zero);
            }
        }
        private int GetDirectionCode(string direction)
        {
            return direction switch
            {
                "Left" => HTLEFT,
                "Right" => HTRIGHT,
                "Top" => HTTOP,
                "TopLeft" => HTTOPLEFT,
                "TopRight" => HTTOPRIGHT,
                "Bottom" => HTBOTTOM,
                "BottomLeft" => HTBOTTOMLEFT,
                "BottomRight" => HTBOTTOMRIGHT,
                _ => HTTOPLEFT
            };
        }
        public string EnteredSecretKey
        {
            get => _enteredSecretKey;
            set
            {
                _enteredSecretKey = value;
                OnPropertyChanged();
            }
        }
        public int PasswordStrength
        {
            get => _passwordStrength;
            set
            {
                _passwordStrength = value;
                PasswordStrengthBar.Value = value;
                PasswordStrengthText.Text = value switch
                {
                    < 30 => "Слабый пароль",
                    < 70 => "Средний пароль",
                    _ => "Надежный пароль"
                };
                PasswordStrengthText.Foreground = value switch
                {
                    < 30 => Brushes.Red,
                    < 70 => Brushes.Orange,
                    _ => Brushes.Green
                };
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_backgroundScale == null) return;

            if (WindowState == WindowState.Maximized)
            {
                var anim = new DoubleAnimation(1.05, TimeSpan.FromMilliseconds(300));
                _backgroundScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                _backgroundScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
            else
            {
                var anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(300));
                _backgroundScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                _backgroundScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }
        public Autorizatsaya(IAuthService authService,
                           INavigationService navigationService,
                           ILogger<Autorizatsaya> logger,
                           IConfigService configService,
                           IEmailTemplateService emailTemplateService,
                           IUserRepository userRepository,
                           IEmailSenderService emailSender,
                           IServiceProvider serviceProvider,
                           ILogger<TwoFactorSetupDialog> twoFactorLogger)
        {
            InitializeComponent();
            DataContext = this;
            // Инициализация полей для анимации
            _backgroundScale = (ScaleTransform)FindName("BackgroundScale") ?? new ScaleTransform(1.0, 1.0);
            _scaleTransform = (ScaleTransform)FindName("ScaleTransform");
            _backgroundBlur = (BlurEffect)FindName("BackgroundBlur");
            _mainBorder = (Border)FindName("MainBorder");
            _toggleIcon = (PackIcon)FindName("ToggleIcon");
            _contentScale = (ScaleTransform)FindName("ContentScale") ??
                  new ScaleTransform { ScaleX = 1.0, ScaleY = 1.0 };

            if (_contentScale != null && !(FindName("ContentScale") is ScaleTransform))
            {
                var contentGrid = (Grid)FindName("ContentGrid");
                if (contentGrid != null)
                {
                    contentGrid.RenderTransform = _contentScale;
                    RegisterName("ContentScale", _contentScale);
                }
            }
            _twoFactorLogger = twoFactorLogger ?? throw new ArgumentNullException(nameof(twoFactorLogger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _emailTemplateService = emailTemplateService;
            _animationManager = new AnimationManager(TimeSpan.FromMilliseconds(400), Dispatcher);

            LoginCommand = new RelayCommand(async () => await LoginAsync());
            VerifyTwoFactorCommand = new RelayCommand(async () => await VerifyTwoFactorCode());
            ConfirmPasswordCommand = new RelayCommand(async () => await ConfirmPassword());
            ChangePasswordCommand = new RelayCommand(async () => await ChangePasswordAsync());

            InitializeBackground();

            _userRepository = userRepository;
            _emailSender = emailSender;
            _serviceProvider = serviceProvider;
            _twoFactorLogger = twoFactorLogger;
        }

        private void ChangePasswordLink_Click(object sender, RoutedEventArgs e)
        {
            SwitchToPanel(ChangePasswordPanel);
        }
        private void RegisterLink_Click(object sender, RoutedEventArgs e) => SwitchToPanel(PasswordConfirmPanel);
        private void GoToRegistrationLink_Click(object sender, RoutedEventArgs e) => SwitchToPanel(RegistrationPanel);
        private void BackButton_Click(object sender, RoutedEventArgs e) => SwitchToPanel(AuthPanel);
        private void SwitchToPanel(UIElement panelToShow)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SwitchToPanel(panelToShow));
                return;
            }
            // Добавляем ChangePasswordPanel в список панелей
            UIElement[] panels = new UIElement[] {
                AuthPanel,
                RegistrationPanel,
                PasswordConfirmPanel,
                ChangePasswordPanel  // Добавляем новую панель
            };
            foreach (var panel in panels)
            {
                if (panel != panelToShow && panel.Visibility == Visibility.Visible)
                {
                    panel.Visibility = Visibility.Collapsed;
                }

            }
            panelToShow.Visibility = Visibility.Visible;
            panelToShow.BeginAnimation(OpacityProperty,
               new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3)));

            // Устанавливаем фокус на соответствующий элемент управления
            if (panelToShow == AuthPanel)
                LoginBox.Focus();
            else if (panelToShow == RegistrationPanel)
                RegFullName.Focus();
            else if (panelToShow == PasswordConfirmPanel)
                SecretKeyBox.Focus();
            else if (panelToShow == ChangePasswordPanel)
               ChangePasswordLoginBox.Focus();
        }
        private void InitializeBackground()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Gifs/camp.gif");
                var image = new BitmapImage(uri);

                ImageBehavior.SetAnimatedSource(BackgroundImage, image);
                ImageBehavior.SetRepeatBehavior(BackgroundImage, new RepeatBehavior(0)); 
                // Бесконечное повторение

                // Инициализация трансформаций и эффекто
                BackgroundImage.RenderTransform = _backgroundScale;
                BackgroundImage.Effect = _backgroundBlur;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки фонового изображения");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri("pack://application:...");
                bitmap.EndInit();
                BackgroundImage.Source = bitmap;
            }
        }
        private async Task LoginAsync()
        {
            if (!ValidateLoginInput(out var error))
            {
                ShowSnackbar(error, true);
                return;
            }
            string login = LoginBox.Text;
            _currentUserLogin = login; // Сохраняем логин при попытке входа
            try
            {
                // 1. Проверяем существует ли пользователь

                if (!await _userRepository.UserExistsAsync(login))
                {
                    ShowSnackbar("Пользователь не найден", true);
                    return;
                }
                // 2. Проверяем блокировку
                if (await _userRepository.IsUserBlockedAsync(login))
                {
                    ShowSnackbar("Аккаунт временно заблокирован", true);
                    return;
                }
                // 3. Проверяем подтверждение аккаунта
                bool isConfirmed = await _userRepository.IsAccountConfirmedAsync(login);
                if (!isConfirmed)
                {
                    // Получаем email пользователя из базы
                    _currentUserEmail = await GetUserEmailAsync(login);
                    if (string.IsNullOrEmpty(_currentUserEmail))
                    {
                        ShowSnackbar("Не удалось получить email пользователя", true);
                        return;
                    }
                    ShowSnackbar("Аккаунт не подтвержден. Введите код подтверждения.", true);
                    SwitchToPanel(PasswordConfirmPanel);
                    return;
                }
                // 4. Проверяем пароль
                var user = await _userRepository.GetUserByLoginAsync(login);
                if (user.Rows.Count == 0 || !BCrypt.Net.BCrypt.Verify(Passbox.Password, user.Rows[0]["password"].ToString()))
                {
                    await _userRepository.IncrementIncorrectAttemptsAsync(login);
                    int attempts = await _userRepository.GetIncorrectAttemptsAsync(login);
                    if (attempts >= 3)
                    {
                        await _userRepository.UpdateBlockStatusAsync(login);
                        ShowSnackbar("Слишком много попыток. Аккаунт заблокирован.", true);
                    }
                    else
                    {
                        ShowSnackbar($"Неверный пароль. Осталось попыток: {3 - attempts}", true);
                    }
                    return;
                }
                // 5. Успешная авторизация
                await _userRepository.ResetIncorrectAttemptsAsync(login);
                await HandleSuccessfulLogin(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа");
                ShowSnackbar("Ошибка входа: " + ex.Message, true);
            }
        }

        private async Task<string> GetUserEmailAsync(string login)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    string query = "SELECT email FROM Users WHERE login = @login"; 
                    return await connection.ExecuteScalarAsync<string>(query, new { login }); // Используем lowercase
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения email пользователя. Login: {login}");
                return null;
            }
        }

        private bool ValidateLoginInput(out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
                error = "Введите логин";
            else if (string.IsNullOrWhiteSpace(Passbox.Password))
                error = "Введите пароль";
            return error == null;
        }
        private async Task VerifyTwoFactorCode()
        {
            if (string.IsNullOrWhiteSpace(TwoFactorCodeBox.Text))
            {
                ShowSnackbar("Введите код подтверждения", true);
                return;
            }
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(_tempSecretKey));
                if (totp.VerifyTotp(TwoFactorCodeBox.Text, out _))
                {
                    var user = await _authService.GetUserAsync(LoginBox.Text);
                    await HandleSuccessfulLogin(user);
                }
                else
                {
                    ShowSnackbar("Неверный код подтверждения", true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка 2FA");
                ShowSnackbar("Ошибка проверки кода: " + ex.Message, true);
            }
        }
        private async Task ConfirmPassword()
        {
            if (string.IsNullOrWhiteSpace(SecretKeyBox.Text))
            {
                ShowSnackbar("Введите секретный ключ", true);
                return;
            }
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    // Проверяем совпадение ключа
                    string checkKeyQuery = "SELECT COUNT(*) FROM Users WHERE login = @Login AND TwoFactorSecret = @SecretKey";
                    using (var cmd = new SqlCommand(checkKeyQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", _currentUserLogin);
                        cmd.Parameters.AddWithValue("@SecretKey", SecretKeyBox.Text);
                        if ((int)await cmd.ExecuteScalarAsync() == 0)
                        {
                            ShowSnackbar("Неверный секретный ключ", true);
                            return;
                        }
                    }
                    // Обновляем статус подтверждения
                    string updateQuery = "UPDATE Users SET password_confirm = 1 WHERE login = @Login";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", _currentUserLogin);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                ShowSnackbar("Пароль успешно подтвержден!", false);
                _navigationService.ShowWindow<MainWindow>();
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подтверждения пароля");
                ShowSnackbar("Ошибка при подтверждении пароля", true);
            }
        }
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button registerButton)) return;
            try
            {
                // Блокировка UI
                registerButton.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
                registerButton.Content = "Регистрация...";

                // Получение данных
                string fullName = RegFullName.Text.Trim();
                string phone = RegPhone.Text.Trim();
                string email = RegEmail.Text.Trim();
                string login = RegLogin.Text.Trim();
                string password = RegPassword.Password;

                var availableProvider = await _emailSender.GetAvailableProviderAsync();
                if (availableProvider == null)
                {
                    ShowSnackbar("Сервис отправки писем временно недоступен. Попробуйте позже.", true);
                    return;
                }
                _currentUserEmail = email;
                _logger.LogInformation("Начало регистрации для: {Email}", email);
                // Валидация
                if (!ValidateRegistrationInput(out var error))
                {
                    _logger.LogWarning("Ошибка валидации: {Error}", error);
                    ShowSnackbar(error, true);
                    return;
                }
                // Генерируем секретный ключ для 2FA
                var secretKey = KeyGeneration.GenerateRandomKey(20);
                var base32Secret = Base32Encoding.ToString(secretKey);
                _tempSecretKey = base32Secret;
                // Хешируем пароль
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                _currentUserLogin = login;
                // Сохраняем пользователя в БД с секретным ключом
                bool dbSuccess = await RegisterUserInDatabase(
                     fullName,
                     phone,
                     email,
                     login,
                     hashedPassword,
                     twoFactorSecret: base32Secret);
                if (dbSuccess)
                {
                    // Показываем диалог настройки Google Authenticator
                    await ShowTwoFactorSetup(base32Secret, login, email);
                    // Больше не отправляем письмо с кодом
                    ShowSnackbar("Регистрация успешна! Настройте Google Authenticator.", false);
                    // Отправляем письмо с кодом подтверждения (не секретным ключом)
                    string confirmationCode = GenerateConfirmationCode();
                    await SaveConfirmationCode(login, confirmationCode);
                    bool emailSent = await SendConfirmationEmailAsync(email, confirmationCode, fullName);
                    if (emailSent)
                    {
                        ShowSnackbar("Регистрация успешна! Подтвердите код из письма.", false);
                        SwitchToPanel(PasswordConfirmPanel);

                    }
                }
                else
                {
                    _logger.LogError($"Не удалось отправить письмо на {email}");
                    ShowSnackbar("Ошибка отправки письма подтверждения", true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации");
                ShowSnackbar($"Ошибка регистрации: {ex.Message}", true);
            }
            finally
            {
                registerButton.IsEnabled = true;
                registerButton.Content = "Зарегистрироваться";
                Mouse.OverrideCursor = null;
            }
        }
        private async Task SaveConfirmationCode(string login, string code)
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                await connection.ExecuteAsync(
                    @"UPDATE Users 
              SET TwoFactorSecret = @Code 
              WHERE login = @Login",
                    new { Code = code, Login = login });
            }
        }
        private async void ResendConfirmationCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentUserLogin))
            {
                ShowSnackbar("Не удалось определить пользователя", true);
                return;
            }

            try
            {
                // Если email еще не сохранен, получаем его из базы
                if (string.IsNullOrEmpty(_currentUserEmail))
                {
                    _currentUserEmail = await GetUserEmailAsync(_currentUserLogin);
                    if (string.IsNullOrEmpty(_currentUserEmail))
                    {
                        ShowSnackbar("Не удалось получить email пользователя", true);
                        return;
                    }
                }
                var newCode = GenerateConfirmationCode();
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    string updateQuery = "UPDATE Users SET TwoFactorSecret = @Code WHERE login = @Login";
                    await connection.ExecuteAsync(updateQuery, new
                    {
                        Code = newCode,
                        Login = _currentUserLogin
                    });
                }

                bool sent = await SendConfirmationEmailAsync(_currentUserEmail, newCode);
                if (sent)
                {
                    ShowSnackbar("Новый код отправлен на вашу почту", false);
                }
                else
                {
                    ShowSnackbar("Не удалось отправить письмо", true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка повторной отправки кода");
                ShowSnackbar($"Ошибка: {ex.Message}", true);
            }
        }
        private string GenerateConfirmationCode()
        {
            // Заменяем Random на криптографически безопасный ключ
            return GenerateSecretKey() ??
                   new Random().Next(100000, 999999).ToString();
        }
        private string GenerateSecretKey()
        {
            try
            {
                var key = KeyGeneration.GenerateRandomKey(20);
                return Base32Encoding.ToString(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации SecretKey");
                return new Random().Next(100000, 999999).ToString(); // Fallback
            }
        }
        private bool ValidateRegistrationInput(out string error)
        {
            error = null;
            var errors = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(RegFullName.Text))
                errors.Add("Введите ФИО");
            if (string.IsNullOrWhiteSpace(RegEmail.Text) || !IsValidEmail(RegEmail.Text))
                errors.Add("Введите корректный email");
            if (string.IsNullOrWhiteSpace(RegPhone.Text) || RegPhone.Text.Length != 11)
                errors.Add("Введите корректный телефон (11 цифр)");
            if (string.IsNullOrWhiteSpace(RegLogin.Text) || RegLogin.Text.Length < 3)
                errors.Add("Логин должен содержать минимум 3 символа");
            if (string.IsNullOrWhiteSpace(RegPassword.Password))
                errors.Add("Введите пароль");
            else if (!CheckPasswordRequirements(RegPassword.Password))
                errors.Add("Пароль не соответствует требованиям");
            if (RegPassword.Password != RegConfirmPassword.Password)
                errors.Add("Пароли не совпадают");
            if (errors.Any())
                error = string.Join("\n", errors);
            return error == null;
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool CheckPasswordRequirements(string password)
        {
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"\d") &&
                   Regex.IsMatch(password, @"[a-z]") &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[\W_]");
        }
        private void UpdatePasswordInfo(string password)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdatePasswordInfo(password));
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                PasswordStrength = 0;
                return;
            }
            bool hasMinLength = password.Length >= 8;
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasSpecial = Regex.IsMatch(password, @"[\W_]");

            LengthCheck.IsChecked = hasMinLength;
            DigitCheck.IsChecked = hasDigit;
            LowerCheck.IsChecked = hasLower;
            UpperCheck.IsChecked = hasUpper;
            SpecialCheck.IsChecked = hasSpecial;

            int strength = 0;
            if (hasMinLength) strength += 20;
            if (password.Length >= 12) strength += 10;
            if (hasDigit) strength += 20;
            if (hasLower) strength += 20;
            if (hasUpper) strength += 20;
            if (hasSpecial) strength += 20;
            strength = Math.Min(100, strength);
            var animation = new DoubleAnimation
            {
                To = strength,
                Duration = TimeSpan.FromSeconds(0.5),
                DecelerationRatio = 0.5
            };
            PasswordStrengthBar.BeginAnimation(ProgressBar.ValueProperty, animation);
            PasswordStrengthText.Text = strength switch
            {
                < 30 => "Слабый",
                < 70 => "Средний",
                < 80 => "Сильный",
                _ => "Надёжный"
            };
            PasswordStrengthText.Foreground = strength switch
            {
                < 30 => Brushes.Red,
                < 70 => Brushes.Orange,
                _ => Brushes.Green
            };
        }
        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (EyeIcon.Kind == PackIconKind.Eye)
            {
                EyeIcon.Kind = PackIconKind.EyeOff;
                RegPasswordVisible.Text = RegPassword.Password;
                RegPassword.Visibility = Visibility.Collapsed;
                RegPasswordVisible.Visibility = Visibility.Visible;
                RegPasswordVisible.Focus();
            }
            else
            {
                EyeIcon.Kind = PackIconKind.Eye;
                RegPassword.Password = RegPasswordVisible.Text;
                RegPasswordVisible.Visibility = Visibility.Collapsed;
                RegPassword.Visibility = Visibility.Visible;
                RegPassword.Focus();
            }
        }
        private void RegPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RegPasswordVisible.Visibility == Visibility.Visible)
            {
                RegPassword.Password = RegPasswordVisible.Text;
                UpdatePasswordInfo(RegPasswordVisible.Text);
            }
        }
        private void RegPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordInfo(RegPassword.Password);
            if (RegPasswordVisible.Visibility == Visibility.Visible)
            {
                RegPasswordVisible.Text = RegPassword.Password;
            }
        }
        private async Task ShowTwoFactorSetup(string secretKey, string login, string email)
        {
            try
            {
                var dialog = new TwoFactorSetupDialog(
                    secretKey,
                    login,
                    email,
                    _authService,
                    _twoFactorLogger
                );
                dialog.ShowNotification += (message, isError) =>
                    ShowSnackbar(message, isError);
                dialog.OnSetupComplete += () =>
                    RootDialogHost.IsOpen = false;
                await RootDialogHost.ShowDialog(dialog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка показа диалога 2FA");
               ShowSnackbar("Ошибка настройки двухфакторной аутентификации", true);
            }
        }

        private async Task HandleSuccessfulLogin(DataTable user)
        {
            try
            {
                var userId = Convert.ToInt32(user.Rows[0]["UserID"]);
                var username = user.Rows[0]["login"].ToString();
                var email = user.Rows[0]["email"]?.ToString();

                // Получаем роли из базы данных
                var roles = await _userRepository.GetUserRolesAsync(userId) ?? new List<string> { "Игрок" };

                // Генерируем токен
                var jwtService = _serviceProvider.GetRequiredService<JwtService>();
                var token = jwtService.GenerateToken(new User
                {
                    Id = userId,
                    Login = username,
                    Email = email,
                    UserRoles = roles.Select(r => new UserRole { Role = new Role { RoleName = r } }).ToList(),
                    Roles = roles
                });

                // Сохраняем данные
                Application.Current.Properties["JwtToken"] = token;

                // Освобождаем ресурсы текущего окна перед созданием нового
                CleanupResources();

                // Создаем главное окно через ServiceProvider
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                if (mainWindow.DataContext is MainWindowViewModel vm)
                {
                    vm.SetUserFromClaims(jwtService.ValidateToken(token));
                }

                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания главного окна");
                ShowSnackbar("Ошибка запуска приложения", true);
            }
        }

        // Метод для определения наивысшей роли
        private string DetermineHighestRole(IEnumerable<string> roles)
                {
                    // Иерархия ролей (от высшей к низшей)
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

        private async Task<bool> SendConfirmationEmailAsync(string email, string code, string fullName = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogError("Email адрес не указан");
                return false;
            }
            // Проверяем, что email принадлежит одному из поддерживаемых доменов
            if (!IsSupportedEmailDomain(email))
            {
               ShowSnackbar("Пожалуйста, используйте почту Mail.ru или Yandex", true);
                return false;
            }
            try
            {
                // Автоматически выбираем доступный провайдер
                var provider = await _emailSender.GetAvailableProviderAsync();
                if (provider == null)
                {
                    ShowSnackbar("Не удалось подключиться к почтовым сервисам. Попробуйте позже.", true);
                    return false;
                }

                bool emailSent = await _emailSender.SendConfirmationEmailAsync(
                    provider,
                    email,
                    code,
                    fullName);

                if (!emailSent)
                {
                    ShowSnackbar("Не удалось отправить письмо подтверждения", true);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки письма на {email}");
                return false;
            }
        }

        private bool IsSupportedEmailDomain(string email)
        {
            var supportedDomains = new[] { "mail.ru", "yandex.ru", "yandex.com" };
            var domain = email.Split('@').LastOrDefault()?.ToLower();
            return supportedDomains.Any(d => domain?.EndsWith(d) == true);
        }
        private async Task HandleFailedLogin(string errorMessage)
        {
            var login = LoginBox.Text;
            await _authService.IncrementIncorrectAttemptsAsync(login);
            if (await _authService.GetIncorrectAttemptsAsync(login) > 3)
            {
                await _authService.BlockUserAsync(login);
                ShowSnackbar("Аккаунт заблокирован из-за 3 неудачных попыток", true);
            }
            else
            {
                ShowSnackbar(errorMessage, true);
           }
        }
        private void ShowSnackbar(string message, bool isError)
        {
            if (string.IsNullOrEmpty(message))
                return;
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Создаем иконку для сообщения
                    var iconKind = isError ? PackIconKind.AlertCircle : PackIconKind.CheckCircle;
                    var icon = new PackIcon
                    {
                        Kind = iconKind,
                        Width = 24,
                        Height = 24,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 8, 0),
                        Foreground = Brushes.White
                    };
                    // Создаем контейнер для сообщения
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    stackPanel.Children.Add(icon);
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    });
                    // Настраиваем цвет фона в зависимости от типа сообщения
                    Snackbar.Background = isError
                        ? new SolidColorBrush(Color.FromRgb(239, 83, 80)) // Красный для ошибок
                        : new SolidColorBrush(Color.FromRgb(67, 160, 71)); // Зеленый для успеха
                    // Добавляем тень для лучшей видимости
                    Snackbar.Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 270,
                        ShadowDepth = 4,
                        Opacity = 0.4,
                        BlurRadius = 8
                    };
                    // Показываем сообщение
                    Snackbar.MessageQueue.Enqueue(
                        stackPanel,
                        null,
                        null,
                        null,
                        false,
                        true,
                        TimeSpan.FromSeconds(4));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка отображения уведомления");
                    // Fallback на обычное сообщение
                    MessageBox.Show(message, isError ? "Ошибка" : "Уведомление",
                        MessageBoxButton.OK,
                        isError ? MessageBoxImage.Error : MessageBoxImage.Information);
                }
            });
        }
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ConfirmAccountAsync();
        }
        private async Task ConfirmAccountAsync()
        {
            if (string.IsNullOrWhiteSpace(SecretKeyBox.Text))
            {
                ShowSnackbar("Введите код подтверждения", true);
                return;
            }
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    // 1. Проверяем код подтверждения

                    string checkKeyQuery = @"SELECT COUNT(*) FROM Users 
                                  WHERE login = @Login 
                                  AND TwoFactorSecret = @SecretKey
                                  AND account_confirmed = 0";
                    int validCodes = await connection.ExecuteScalarAsync<int>(checkKeyQuery, new
                    {
                        Login = _currentUserLogin,
                        SecretKey = SecretKeyBox.Text.Trim()
                    });
                    if (validCodes == 0)
                    {
                        ShowSnackbar("Неверный код или аккаунт уже подтверждён", true);
                        return;
                    }
                    // 2. Подтверждаем аккаунт (устанавливаем только account_confirmed)
                    string confirmQuery = @"UPDATE Users 
                                SET account_confirmed = 1 
                                WHERE login = @Login";
                    await connection.ExecuteAsync(confirmQuery, new
                    {
                        Login = _currentUserLogin
                    });
                    ShowSnackbar("Аккаунт успешно подтверждён!", false);

                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.Show();
                    this.Close();
                }

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Ошибка подтверждения аккаунта");

                ShowSnackbar($"Ошибка при подтверждении: {ex.Message}", true);

            }

        }
        private async Task<bool> RegisterUserInDatabase(
            string fullName, string phone, string email,
            string login, string passwordHash, string twoFactorSecret)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    // Начинаем транзакцию
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Вставляем пользователя
                            string insertUserQuery = @"
                        INSERT INTO Users (
                            Full_name, 
                            Number_telephone, 
                            email,
                            login, 
                            password, 
                            Date_Auto, 
                            TwoFactorSecret,
                            account_confirmed,
                            password_confirm,
                            user_blocked,
                            Incorrect_pass
                        )
                        VALUES (
                            @FullName, 
                            @PhoneNumber, 
                            @Email,
                            @Login, 
                            @Password, 
                            GETDATE(), 
                            @TwoFactorSecret,
                            0,
                            0,
                            0,
                            0
                        );
                        SELECT SCOPE_IDENTITY();";

                            int userId = await connection.ExecuteScalarAsync<int>(
                                insertUserQuery,
                                new
                                {
                                    FullName = fullName,
                                    PhoneNumber = phone,
                                    Email = email,
                                    Login = login,
                                    Password = passwordHash,
                                    TwoFactorSecret = twoFactorSecret
                                },
                                transaction);

                            // 2. Добавляем роль "Игрок" (RoleID = 1)
                            string assignRoleQuery = @"
                                INSERT INTO UserRoles (UserID, RoleID)
                                VALUES (@UserId, 1)";

                            await connection.ExecuteAsync(
                                assignRoleQuery,
                                new { UserId = userId },
                                transaction);

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации в БД");
                return false;
            }
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute ?? (() => true);
            }
            public bool CanExecute(object parameter) => _canExecute();
            public void Execute(object parameter) => _execute();
        }
        private async void ResendCodeLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentUserLogin))
                {
                    ShowSnackbar("Сессия истекла. Войдите снова.", true);
                    SwitchToPanel(AuthPanel);
                    return;
                }
                var newCode = new Random().Next(100000, 999999).ToString();

                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    string updateQuery = @"
                            UPDATE Users 
                            SET TwoFactorSecret = @NewCode 
                            WHERE login = @Login";

                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewCode", newCode);
                        cmd.Parameters.AddWithValue("@Login", _currentUserLogin);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                bool sent = await SendConfirmationEmailAsync(_currentUserEmail, newCode);

                ShowSnackbar("Новый код отправлен на вашу почту", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка повторной отправки кода");

                ShowSnackbar("Ошибка отправки кода", true);
            }
        }

        private async void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowTransitionInProgress) return;
            _isWindowTransitionInProgress = true;

            try
            {
                await AnimationHelper.ToggleFullscreenMode(
                    this,
                    _contentScale,
                    _backgroundScale,
                    _backgroundBlur,
                    _toggleIcon,
                    _animationDuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка переключения режима окна");
            }
            finally
            {
                _isWindowTransitionInProgress = false;
            }
        }

     public class AnimationManager
        {
            private readonly TimeSpan _duration;
            private readonly IEasingFunction _easingFunction;
            private bool _isAnimating;
            private readonly Dispatcher _dispatcher;
            public AnimationManager(TimeSpan duration, Dispatcher dispatcher = null)
            {
                _duration = duration;
                _easingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut };
                _dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
            }

            public async Task RunAnimationAsync(Action<Action> animationSetup)
            {
                if (_isAnimating) return;
                _isAnimating = true;
                var tcs = new TaskCompletionSource<bool>();
                try
                {
                    _dispatcher.Invoke(() =>
                    {
                        try
                        {
                            animationSetup(() => tcs.TrySetResult(true));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    });

                    await tcs.Task;
                }
                finally
                {
                    _isAnimating = false;
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && WindowState == WindowState.Normal)
            {
                DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Анимация закрытия окна
            var closingAnimation = new DoubleAnimation(0,
                new Duration(TimeSpan.FromSeconds(0.2)));
            closingAnimation.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, closingAnimation);
        }
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            // Обновляем состояние кнопки при изменении состояния окна
            if (WindowState == WindowState.Maximized)
            {
                _isFullscreen = true;
                if (ToggleFullscreenButton != null)
                    ((PackIcon)ToggleFullscreenButton.Content).Kind = PackIconKind.WindowRestore;
            }
            else if (WindowState == WindowState.Normal)
            {
                _isFullscreen = false;
                if (ToggleFullscreenButton != null)
                    ((PackIcon)ToggleFullscreenButton.Content).Kind = PackIconKind.WindowMaximize;
            }
        }
        private async Task ChangePasswordAsync()
        {
            if (!ValidateChangePasswordInput(out var error))
            {
                ShowSnackbar(error, true);
                return;
            }

            string login = ChangePasswordLoginBox.Text;

            try
            {
                // Получаем полную информацию о пользователе
                var user = await _userRepository.GetUserByLoginAsync(login);

                if (user.Rows.Count == 0)
                {
                    ShowSnackbar("Пользователь не найден", true);
                    return;
                }

                bool hasTwoFactorEnabled = Convert.ToBoolean(user.Rows[0]["password_confirm"]);
                bool hasTwoFactorKey = !string.IsNullOrEmpty(user.Rows[0]["TwoFactorSecret"]?.ToString());

                // Если пользователь подтвердил 2FA
                if (hasTwoFactorEnabled && hasTwoFactorKey)
                {
                    if (TwoFactorCodeChangePasswordBox.Visibility != Visibility.Visible)
                    {
                        TwoFactorCodeChangePasswordBox.Visibility = Visibility.Visible;
                        TwoFactorCodeChangePasswordBox.Focus();
                        ShowSnackbar("Введите код из Google Authenticator", false);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(TwoFactorCodeChangePasswordBox.Text))
                    {
                        ShowSnackbar("Введите код подтверждения", true);
                        return;
                    }

                    bool isCodeValid = await _authService.VerifyTwoFactorCodeAsync(login, TwoFactorCodeChangePasswordBox.Text);
                    if (!isCodeValid)
                    {
                        ShowSnackbar("Неверный код подтверждения", true);
                        return;
                    }
                }
                else
                {
                    // Проверяем старый пароль для неподтвержденных пользователей
                    string storedHash = user.Rows[0]["password"].ToString();
                    if (!BCrypt.Net.BCrypt.Verify(OldPasswordBox.Password, storedHash))
                    {
                        ShowSnackbar("Неверный старый пароль", true);
                        return;
                    }
                }

                // Если все проверки пройдены - меняем пароль
                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPasswordBox.Password);
                bool success = await _userRepository.ChangePasswordAsync(login, newPasswordHash);

                if (success)
                {
                    TwoFactorCodeChangePasswordBox.Clear();
                    TwoFactorCodeChangePasswordBox.Visibility = Visibility.Collapsed;
                    ShowSnackbar("Пароль успешно изменен!", false);
                    SwitchToPanel(AuthPanel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка смены пароля");
                ShowSnackbar($"Ошибка смены пароля: {ex.Message}", true);
            }
        }

        public async Task<bool> CheckTimeSynchronization(string login)
        {
            try
            {
                var secretKey = await GetTwoFactorSecretAsync(login);
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                return totp.VerifyTotp(totp.ComputeTotp(), out _, new VerificationWindow(2, 2));
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetTwoFactorSecretAsync(string login)
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                return await connection.ExecuteScalarAsync<string>(
                    "SELECT TwoFactorSecret FROM Users WHERE login = @Login",
                    new { Login = login });
            }
        }

        private bool ValidateChangePasswordInput(out string error)
        {
            error = null;
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ChangePasswordLoginBox.Text))
                errors.Add("Введите логин");
            if (string.IsNullOrWhiteSpace(OldPasswordBox.Password))
                errors.Add("Введите старый пароль");
            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
                errors.Add("Введите новый пароль");
            else if (!CheckPasswordRequirements(NewPasswordBox.Password))
                errors.Add("Новый пароль не соответствует требованиям");
            if (NewPasswordBox.Password != ConfirmNewPasswordBox.Password)
                errors.Add("Новые пароли не совпадают");

            if (errors.Any())
                error = string.Join("\n", errors);
            return error == null;
        }
        private void BackToAuth_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем состояние панели
            TwoFactorCodeChangePasswordBox.Visibility = Visibility.Collapsed;
            TwoFactorCodeChangePasswordBox.Text = "";
            SwitchToPanel(AuthPanel);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Освобождаем ресурсы анимации
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);
            BackgroundImage.Source = null;

            // Освобождаем другие ресурсы
            _backgroundBlur = null;
            _backgroundScale = null;
            _contentScale = null;
            CleanupResources();
        }

        private void CleanupResources()
        {
            // Останавливаем анимации GIF
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);

            // Очищаем подписки на события

            // Освобождаем графические ресурсы
            BackgroundImage.Source = null;

            // Принудительный сбор мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void CleanupAnimations()
        {
            // Очистка анимаций
            BeginAnimation(OpacityProperty, null);
            RenderTransform = null;
            Effect = null;

            // Для всех дочерних элементов
            foreach (var child in LogicalTreeHelper.GetChildren(this).OfType<FrameworkElement>())
            {
                child.BeginAnimation(OpacityProperty, null);
                child.RenderTransform = null;
                child.Effect = null;
            }
        }

    }
}
