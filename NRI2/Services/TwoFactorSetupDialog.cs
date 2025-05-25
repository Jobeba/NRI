using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;
using MaterialDesignThemes.Wpf;
using NRI.Classes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace NRI
{
    public partial class TwoFactorSetupDialog : UserControl
    {
        public event Action<string, bool> ShowNotificationRequested;
        public event Action OnSetupComplete; // Добавлено событие завершения настройки

        private readonly ILogger _logger;
        private readonly IAuthService _authService;
        private string _secretKey;
        private string _login;
        private string _email;

        // Конструктор без параметров для Designer
        public TwoFactorSetupDialog()
        {
            InitializeComponent();

            VerificationCodeBox.PreviewTextInput += VerificationCodeBox_PreviewTextInput;
            VerificationCodeBox.TextChanged += VerificationCodeBox_TextChanged;
        }

        // Основной конструктор
        public TwoFactorSetupDialog(string secretKey, string login, string email,
                                  IAuthService authService, ILogger logger)
        {
            InitializeComponent();
            Initialize(secretKey, login, email);
            _authService = authService;
            _logger = logger;
        }
        // Метод инициализации
        public void Initialize(string secretKey, string login, string email)
        {
            _secretKey = secretKey;
            _login = login;
            _email = email;

            QrCodeImage.Source = GenerateQrCode(secretKey, login, email);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            QrCodeImage.Source = GenerateQrCode(_secretKey, _login, _email);
        }

        private BitmapImage GenerateQrCode(string secretKey, string login, string email)
        {
            try
            {
                string issuer = "NRI System";
                string qrContent = $"otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}";

                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
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
                return null;
            }
        }
        private async void VerifyCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VerificationCodeBox.Text) || VerificationCodeBox.Text.Length != 6)
            {
                ShowNotification?.Invoke("Введите 6-значный код из приложения", true);
                return;
            }

            try
            {
                // Сначала сохраняем секретный ключ
                bool saveResult = await _authService.SaveTwoFactorSecretAsync(_login, _secretKey);

                if (!saveResult)
                {
                    ShowNotification?.Invoke("Не удалось сохранить настройки 2FA", true);
                    return;
                }

                // Затем проверяем код
                bool isConfirmed = await _authService.ConfirmAccountWithTwoFactorAsync(_login, VerificationCodeBox.Text);

                if (isConfirmed)
                {
                    ShowNotification?.Invoke("Аккаунт успешно подтвержден!", false);
                    OnSetupComplete?.Invoke();
                }
                else
                {
                    ShowNotification?.Invoke("Неверный код подтверждения", true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подтверждения 2FA");
                ShowNotification?.Invoke("Ошибка подтверждения кода", true);
            }
        }

        public async Task ShowNotificationAsync(string message, bool isError)
        {
            if (ShowNotificationRequested != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ShowNotificationRequested.Invoke(message, isError);
                });
            }
        }

        private void FinishSetup_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
