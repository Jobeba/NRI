using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using QRCoder;
using System.Threading.Tasks;
using System.Windows.Input;

using WpfBrushes = System.Windows.Media.Brushes;
using WpfFontFamily = System.Windows.Media.FontFamily;
using WpfImage = System.Windows.Controls.Image;

namespace NRI
{
    public partial class Registrasya : Window, INotifyPropertyChanged
    {
        private int _passwordStrength = 0;
        private readonly ILogger<Registrasya> _logger;
        private readonly IConfigService _configService;

        public Registrasya(ILogger<Registrasya> logger, IConfigService configService)
        {
            InitializeComponent();
            _logger = logger;
            _configService = configService;
            DataContext = this;

            // Инициализация обработчиков событий
            RegisterButton.Click += RegisterButton_Click_Handler;
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            _passwordTextBox.TextChanged += _passwordTextBox_TextChanged;
            TogglePasswordButton.Click += TogglePasswordVisibility;

            if (RegistrationHyperlink != null)
            {
                RegistrationHyperlink.Click += Hyperlink_Click;
            }
        }

        public int PasswordStrength
        {
            get => _passwordStrength;
            set
            {
                if (_passwordStrength != value)
                {
                    _passwordStrength = value;
                    OnPropertyChanged(nameof(PasswordStrength));
                    OnPropertyChanged(nameof(PasswordHint));
                }
            }
        }

        public string PasswordHint => PasswordStrength switch
        {
            < 30 => "Слабый пароль",
            < 70 => "Средний пароль",
            _ => "Надежный пароль"
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string password = passwordBox.Password;
            UpdatePasswordInfo(password);
        }

        private void _passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string password = _passwordTextBox.Text;
            UpdatePasswordInfo(password);
        }

        private void UpdatePasswordInfo(string password)
        {
            PasswordStrength = CalculatePasswordStrength(password);
            UpdatePasswordStrength(PasswordStrength);
            UpdatePasswordRequirements(password);
        }

        private void UpdatePasswordStrength(int newStrength)
        {
            var animation = new DoubleAnimation
            {
                To = newStrength,
                Duration = TimeSpan.FromSeconds(0.7)
            };
            passwordStrengthProgressBar.BeginAnimation(RangeBase.ValueProperty, animation);
        }

        private void UpdatePasswordRequirements(string password)
        {
            PasswordLengthCheckBox.IsChecked = password.Length >= 6;
            PasswordDigitCheckBox.IsChecked = Regex.IsMatch(password, @"\d");
            PasswordLowercaseCheckBox.IsChecked = Regex.IsMatch(password, @"[a-z]");
            PasswordUppercaseCheckBox.IsChecked = Regex.IsMatch(password, @"[A-Z]");
            PasswordSpecialCharCheckBox.IsChecked = Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");
        }

        private int CalculatePasswordStrength(string password)
        {
            int strength = 0;

            if (password.Length >= 6) strength += 20;
            if (Regex.IsMatch(password, @"\d")) strength += 20;
            if (Regex.IsMatch(password, @"[a-z]")) strength += 20;
            if (Regex.IsMatch(password, @"[A-Z]")) strength += 20;
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]")) strength += 20;

            return Math.Min(strength, 100);
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (_passwordTextBox == null || passwordBox == null)
            {
                MessageBox.Show("Ошибка: элементы управления паролем не инициализированы.");
                return;
            }

            if (_passwordTextBox.Visibility == Visibility.Visible)
            {
                var hideAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.2)
                };
                hideAnimation.Completed += (s, _) =>
                {
                    _passwordTextBox.Visibility = Visibility.Collapsed;
                    passwordBox.Visibility = Visibility.Visible;
                    passwordBox.Password = _passwordTextBox.Text;
                };
                _passwordTextBox.BeginAnimation(UIElement.OpacityProperty, hideAnimation);
            }
            else
            {
                _passwordTextBox.Text = passwordBox.Password;
                _passwordTextBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;

                var showAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1)
                };
                _passwordTextBox.BeginAnimation(UIElement.OpacityProperty, showAnimation);
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^\+?\d{10,15}$");
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 &&
                   Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,}$");
        }

        private void ShowSnackbar(string message, bool isError = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var content = new TextBlock
                {
                    Text = message,
                    Foreground = isError ? WpfBrushes.White : WpfBrushes.Black,
                    FontFamily = new WpfFontFamily("Segoe UI"),
                    TextWrapping = TextWrapping.Wrap
                };

                Snackbar.Background = isError ? WpfBrushes.DarkRed : WpfBrushes.LightGreen;
                Snackbar.Foreground = isError ? WpfBrushes.White : WpfBrushes.Black;

                Snackbar.MessageQueue.Enqueue(
                    content,
                    null,
                    null,
                    null,
                    false,
                    true,
                    TimeSpan.FromSeconds(3));
            });
        }

        private async Task ShowRegistrationSuccess(string secretKey, string login)
        {
            try
            {
                var stackPanel = new StackPanel { Margin = new Thickness(10) };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = "Регистрация успешна!",
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var keyPanel = new StackPanel { Orientation = Orientation.Horizontal };
                keyPanel.Children.Add(new TextBlock
                {
                    Text = "Секретный ключ:",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 5, 0)
                });

                var keyBox = new TextBox
                {
                    Text = secretKey,
                    IsReadOnly = true,
                    FontFamily = new WpfFontFamily("Consolas"),
                    MinWidth = 200
                };

                var copyBtn = new Button
                {
                    Content = "Копировать",
                    Margin = new Thickness(5, 0, 0, 0),
                    Command = new RelayCommand(() => Clipboard.SetText(secretKey))
                };

                keyPanel.Children.Add(keyBox);
                keyPanel.Children.Add(copyBtn);
                stackPanel.Children.Add(keyPanel);

                var qrImage = new WpfImage
                {
                    Source = GenerateQrCodeBitmap(secretKey, login),
                    Width = 200,
                    Height = 200,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                stackPanel.Children.Add(new TextBlock
                {
                    Text = "Отсканируйте QR-код в Google Authenticator:",
                    Margin = new Thickness(0, 10, 0, 5)
                });
                stackPanel.Children.Add(qrImage);

                stackPanel.Children.Add(new TextBlock
                {
                    Text = "1. Установите Google Authenticator\n" +
                           "2. Отсканируйте QR-код или введите ключ\n" +
                           "3. При входе введите код из приложения",
                    Margin = new Thickness(0, 10, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                });

                // Используем конкретный экземпляр DialogHost
                await RootDialogHost.ShowDialog(stackPanel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка показа QR-кода");
                MessageBox.Show($"Ошибка генерации QR-кода: {ex.Message}");
            }
        }

        private BitmapImage GenerateQrCodeBitmap(string secretKey, string login)
        {
            try
            {
                string uri = $"otpauth://totp/NRI:{login}?secret={secretKey}&issuer=NR";

                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);

                    using (var qrCode = new QRCode(qrCodeData))
                    using (var bitmap = qrCode.GetGraphic(20))
                    using (var memory = new MemoryStream())
                    {
                        bitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memory;
                        bitmapImage.EndInit();

                        return bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации QR-кода");
                throw;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new Autorizatsaya(
                App.ServiceProvider.GetRequiredService<IAuthService>(),
                App.ServiceProvider.GetRequiredService<INavigationService>(),
                App.ServiceProvider.GetRequiredService<ILogger<Autorizatsaya>>());
            
            authWindow.Show();
            this.Close();
        }

        private async void RegisterButton_Click_Handler(object sender, RoutedEventArgs e)
        {
            await RegisterUserAsync();
        }

        private async Task RegisterUserAsync()
        {
            string fullName = FullName_textbox.Text;
            string phoneNumber = Number_telephone.Text;
            string login = Login_textbox.Text;
            string password = string.IsNullOrEmpty(_passwordTextBox.Text)
                ? passwordBox.Password
                : _passwordTextBox.Text;

            if (!ValidateInputs(fullName, phoneNumber, login, password))
                return;

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var secretKey = KeyGeneration.GenerateRandomKey(20);
                var base32Secret = Base32Encoding.ToString(secretKey);

                if (!await RegisterUserInDatabase(fullName, phoneNumber, login, hashedPassword, base32Secret))
                    return;

                await ShowRegistrationSuccess(base32Secret, login);
                ShowSnackbar("Регистрация успешно завершена!", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации");
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
            }
        }

        private bool ValidateInputs(string fullName, string phoneNumber, string login, string password)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                ShowSnackbar("Введите ваше ФИО", true);
                FullName_textbox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(phoneNumber))
            {
                ShowSnackbar("Введите номер телефона", true);
                Number_telephone.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(login))
            {
                ShowSnackbar("Придумайте логин", true);
                Login_textbox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowSnackbar("Придумайте пароль", true);
                passwordBox.Focus();
                return false;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                ShowSnackbar("Некорректный номер телефона", true);
                return false;
            }

            if (!IsValidPassword(password))
            {
                ShowSnackbar("Пароль должен содержать минимум 6 символов, включая цифры, заглавные и строчные буквы", true);
                return false;
            }

            return true;
        }

        private async Task<bool> RegisterUserInDatabase(string fullName, string phoneNumber, string login, string hashedPassword, string secretKey)
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                await connection.OpenAsync();

                // Проверка уникальности логина
                string checkLoginQuery = "SELECT COUNT(*) FROM Users WHERE login = @Login";
                using (var checkCmd = new SqlCommand(checkLoginQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Login", login);
                    if ((int)await checkCmd.ExecuteScalarAsync() > 0)
                    {
                        ShowSnackbar("Логин уже занят", true);
                        return false;
                    }
                }

                // Регистрация пользователя
                string insertQuery = @"
                    INSERT INTO Users (Full_name, Number_telephone, login, password, Date_Auto, TwoFactorSecret)
                    VALUES (@FullName, @PhoneNumber, @Login, @Password, GETDATE(), @TwoFactorSecret)
                    SELECT SCOPE_IDENTITY()";

                int userId;
                using (var insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@FullName", fullName);
                    insertCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    insertCmd.Parameters.AddWithValue("@Login", login);
                    insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
                    insertCmd.Parameters.AddWithValue("@TwoFactorSecret", secretKey);

                    userId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
                }

                // Назначение роли
                string roleQuery = @"
                    INSERT INTO UserRoles (UserID, RoleID)
                    VALUES (@UserID, (SELECT RoleID FROM Roles WHERE RoleName = 'Игрок'))";

                using (var roleCmd = new SqlCommand(roleQuery, connection))
                {
                    roleCmd.Parameters.AddWithValue("@UserID", userId);
                    await roleCmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute ?? (() => true);
            }

            public bool CanExecute(object parameter) => _canExecute();
            public void Execute(object parameter) => _execute();

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}
