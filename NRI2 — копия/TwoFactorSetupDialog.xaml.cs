using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OtpNet;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using System.Linq;

namespace NRI
{
    public partial class TwoFactorSetupDialog : UserControl
    {
        public string SecretKey { get; private set; }

        public event Action<string, bool> ShowNotification;

        public TwoFactorSetupDialog(string secretKey, string login, string email,
                                  IAuthService authService, ILogger<TwoFactorSetupDialog> logger)
        {
            InitializeComponent();

            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _login = login ?? throw new ArgumentNullException(nameof(login));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            SecretKey = _secretKey;
            DataContext = this;

            Loaded += OnControlLoaded;
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                QrCodeImage.Source = GenerateQrCode(_secretKey, _email);
                VerificationCodeBox.Focus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки QR-кода");
                ShowNotification?.Invoke("Ошибка загрузки QR-кода", true);
            }
        }

        private void VerificationCodeBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void VerificationCodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(" ", "");


            if (text.Length > 3)
            {
                textBox.Text = text.Insert(3, " ");
                textBox.CaretIndex = text.Length + 1; // Перемещаем курсор после пробела
            }
            else
            {
                textBox.Text = text;
            }
        }

        private BitmapImage GenerateQrCode(string secretKey, string email)
        {
            try
            {
                string issuer = "NRI";
                string qrContent = $"otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}";

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);

                using (MemoryStream memory = new MemoryStream())
                {
                    qrCodeImage.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации QR-кода");
                throw;
            }
        }
    }
}
