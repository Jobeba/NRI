using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Data.SqlClient;
using System.IO;
using BCrypt.Net;

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

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!");
                return;
            }

            try
            {
                // Хеширование пароля
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());

                // Регистрация пользователя
                using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FLP8EG6\\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False"))
                {
                    connection.Open();

                    // Проверка уникальности логина
                    string checkLoginQuery = "SELECT COUNT(*) FROM Users WHERE login = @login";
                    using (SqlCommand checkLoginCommand = new SqlCommand(checkLoginQuery, connection))
                    {
                        checkLoginCommand.Parameters.AddWithValue("@login", login);
                        int userCount = (int)checkLoginCommand.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует!");
                            return;
                        }
                    }

                    // Вставка нового пользователя
                    string insertUserQuery = @"
                        INSERT INTO Users (Full_name, Number_telephone, login, password)
                        VALUES (@FullName, @PhoneNumber, @Login, @Password);
                        SELECT SCOPE_IDENTITY();"; // Получаем ID нового пользователя

                    int userID;
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@FullName", fullName);
                        insertUserCommand.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        insertUserCommand.Parameters.AddWithValue("@Login", login);
                        insertUserCommand.Parameters.AddWithValue("@Password", hashedPassword);

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

                    MessageBox.Show("Пользователь успешно зарегистрирован и ему назначена роль 'Игрок'!");
                    Log($"Пользователь {login} успешно зарегистрирован.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message);
                Log($"Ошибка при регистрации пользователя {login}: {ex.Message}");
            }
        }

        // Валидация номера телефона
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length > 11)
            {
                return false;
            }

            string pattern = @"^\+?\d{10,15}$"; // Пример: +79991234567 или 89991234567
            return Regex.IsMatch(phoneNumber, pattern);
        }

        // Логирование
        private void Log(string message)
        {
            string logFilePath = "registration_log.txt";
            string logMessage = $"{DateTime.Now}: {message}\n";

            File.AppendAllText(logFilePath, logMessage);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            DiceRoller reg = new DiceRoller();
            reg.Show();
            this.Close();
        }
    }
}