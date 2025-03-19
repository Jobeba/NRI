using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Data.SqlClient;
using System.IO;
using BCrypt.Net;
using Microsoft.Extensions.DependencyInjection;
using OtpNet;

namespace NRI
{
    public partial class Registrasya : Window
    {
        public Registrasya()
        {
            InitializeComponent();
        }

        private void Registr_button_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullName_textbox.Text;
            string phoneNumber = Number_telephone.Text;
            string login = Login_textbox.Text;
            string password = passwordBox.Password;

            // Валидация данных
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phoneNumber) ||
                string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполнены не все поля!");
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                MessageBox.Show("Некорректный номер телефона!");
                return;
            }

            if (!IsValidPassword(password))
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов, включая цифры, заглавные и строчные буквы!");
                return;
            }

            try
            {
                // Хеширование пароля
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());

                // Генерация секретного ключа для 2FA
                string secretKey = GenerateTwoFactorSecret();

                // Регистрация пользователя
                using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FLP8EG6\\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False"))
                {
                    connection.Open();

                    // Проверка уникальности логина
                    string checkLoginQuery = "SELECT COUNT(*) FROM Users WHERE login = @Login";
                    using (SqlCommand checkLoginCommand = new SqlCommand(checkLoginQuery, connection))
                    {
                        checkLoginCommand.Parameters.AddWithValue("@Login", login);
                        int userCount = (int)checkLoginCommand.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует!");
                            return;
                        }
                    }

                    // Вставка нового пользователя
                    string insertUserQuery = @"
                INSERT INTO Users (Full_name, Number_telephone, login, password, Date_Auto, TwoFactorSecret)
                VALUES (@FullName, @PhoneNumber, @Login, @Password, GETDATE(), @TwoFactorSecret)
                SELECT SCOPE_IDENTITY()"; // Получаем ID нового пользователя

                    int userID;
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@FullName", fullName);
                        insertUserCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        insertUserCommand.Parameters.AddWithValue("@Login", login);
                        insertUserCommand.Parameters.AddWithValue("@Password", hashedPassword);
                        insertUserCommand.Parameters.AddWithValue("@TwoFactorSecret", secretKey);

                        userID = Convert.ToInt32(insertUserCommand.ExecuteScalar());
                    }

                    // Назначение роли "Игрок"
                    string assignRoleQuery = @"
                INSERT INTO UserRoles (UserID, RoleID)
                VALUES (@UserID, (SELECT RoleID FROM Roles WHERE RoleName = 'Игрок'))";

                    using (SqlCommand assignRoleCommand = new SqlCommand(assignRoleQuery, connection))
                    {
                        assignRoleCommand.Parameters.AddWithValue("@UserID", userID);
                        assignRoleCommand.ExecuteNonQuery();
                    }

                    // Генерация URL для QR-кода
                    string qrCodeUrl = GenerateQrCodeUrl(secretKey, login, "MyApp");

                    MessageBox.Show("Пользователь успешно зарегистрирован и ему назначена роль 'Игрок'!\nСекретный ключ для 2FA: " + secretKey);
                    Log($"Пользователь {login} успешно зарегистрирован.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message);
                Log($"Ошибка при регистрации пользователя {login}: {ex.Message}", "ERROR");
            }
        }

        public string GenerateTwoFactorSecret(int keyLength = 20)
        {
            var secretKey = KeyGeneration.GenerateRandomKey(keyLength); // Генерация случайного ключа
            return Base32Encoding.ToString(secretKey); // Преобразование в строку Base32
        }

        public string GenerateQrCodeUrl(string secretKey, string userEmail, string appName)
        {
            string encodedAppName = Uri.EscapeDataString(appName);
            string encodedUserEmail = Uri.EscapeDataString(userEmail);
            return $"otpauth://totp/{encodedAppName}:{encodedUserEmail}?secret={secretKey}&issuer={encodedAppName}";
        }

        public bool ValidateTwoFactorCode(string secretKey, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            return totp.VerifyTotp(code, out _); // Проверка кода
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length > 11)
            {
                return false;
            }

            string pattern = @"^\+?\d{10,15}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 6)
                return false;

            // Проверка на наличие цифр, заглавных и строчных букв
            return Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,}$");
        }

        private void Log(string message, string level = "INFO")
        {
            string logFilePath = "registration_log.txt";
            string logMessage = $"{DateTime.Now} [{level}]: {message}\n";

            File.AppendAllText(logFilePath, logMessage);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = App.ServiceProvider.GetRequiredService<Autorizatsaya>();
            registrationWindow.Show();
            this.Close();
        }
    }
}