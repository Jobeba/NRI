using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Collections;
using System.Security.Cryptography;

namespace Bully_Maguire
{
    public partial class ChangePasswordWindow : Window
    {
        private string _login;
        private const string ConnectionString = @"Data Source=209-U\SQLEXPRESS209;Initial Catalog=Bully Maguire1;Integrated Security=True;Encrypt=False";
        private int _failedAttempts = 0; // Счетчик неправильных попыток

        public ChangePasswordWindow(string login)
        {
            InitializeComponent();
            _login = login;
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Проверка заполненности полей
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageTextBlock.Text = "Все поля должны быть заполнены.";
                return;
            }

            // Проверка совпадения нового пароля и его подтверждения
            if (newPassword != confirmPassword)
            {
                MessageTextBlock.Text = "Новый пароль и подтверждение не совпадают.";
                return;
            }

            // Проверка текущего пароля и смена пароля
            if (ChangePassword(currentPassword, newPassword))
            {
                // Обновляем статус смены пароля
                UpdatePasswordChangedStatus(_login);

                MessageTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                MessageTextBlock.Text = "Пароль успешно изменен!";

                // Открываем окно администратора
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show(); // Открываем окно администратора
                this.Close(); // Закрываем текущее окно
            }
            else
            {
                _failedAttempts++;
                MessageTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                MessageTextBlock.Text = "Неверный текущий пароль.";

                // Отладочное сообщение
                MessageBox.Show($"Количество неправильных попыток: {_failedAttempts}");

                if (_failedAttempts >= 3)
                {
                    BlockUser(); // Блокируем пользователя
                    MessageBox.Show("Вы заблокированы. Обратитесь к администратору.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown(); // Закрываем приложение
                }
            }
        }

        private void UpdatePasswordChangedStatus(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE Users SET IsPasswordChanged = 1 WHERE Login = @login"; // Устанавливаем статус смены пароля
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@login", login);
                    updateCommand.ExecuteNonQuery();
                }
            }
        }






        private bool ChangePassword(string currentPassword, string newPassword)
        {
            if (IsUserBlocked(_login)) // Проверяем, заблокирован ли пользователь
    { 
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору.", "Блокировка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Password FROM Users WHERE Login = @login"; // Проверяем текущий пароль
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", _login);
                    var storedPassword = command.ExecuteScalar();

                    if (storedPassword != null && storedPassword.ToString() == currentPassword)
                    {
                        // Смена пароля
                        string updateQuery = "UPDATE Users SET Password = @newPassword WHERE Login = @login";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@newPassword", newPassword);
                            updateCommand.Parameters.AddWithValue("@login", _login);
                            updateCommand.ExecuteNonQuery();
                            return true; // Пароль успешно изменен
                        }
                    }
                    return false; // Неверный текущий пароль
                }
            }
        }

        private bool IsUserBlocked(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT IsBlocked FROM Users WHERE Login = @login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    var isBlocked = command.ExecuteScalar();
                    return isBlocked != null && (bool)isBlocked; // Проверяем статус блокировки
                }
            }
        }



        private void BlockUser()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string blockUserQuery = "UPDATE Users SET IsBlocked = 1 WHERE Login = @login"; // Устанавливаем значение 1 для блокировки
                using (SqlCommand blockUserCommand = new SqlCommand(blockUserQuery, connection))
        {
                    blockUserCommand.Parameters.AddWithValue("@login", _login); // Используем _login
                    blockUserCommand.ExecuteNonQuery();
                }
            }
        }


        private void SkipChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка роли пользователя
            if (CheckUserRole(_login) == "admin")
            {   
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show(); // Открываем окно администратора
                this.Close(); // Закрываем текущее окно
            }
            else
            {
                MessageBox.Show("Эта программа предназначена только для администраторов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string CheckUserRole(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Role FROM Users WHERE Login = @login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    var role = command.ExecuteScalar();
                    return role?.ToString();
                }
            }
        }
    }
}