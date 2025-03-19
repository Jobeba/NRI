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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Collections;
using System.Security.Cryptography;

namespace Bully_Maguire
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = @"Data Source=209-U\SQLEXPRESS209;Initial Catalog=Bully Maguire1;Integrated Security=True;Encrypt=False";


        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Пожалуйста, заполните все поля.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            bool isAuthenticated = AuthenticateUser(login, password);

            if (isAuthenticated)
            {
                // Проверяем, изменил ли пользователь пароль
                if (HasUserChangedPassword(login))
        {
                    // Пользователь авторизован и изменил пароль, открываем окно администратора
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close(); // Закрываем текущее окно
                }
        else
                {
                    // Открываем форму смены пароля
                    ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(login);
                    changePasswordWindow.ShowDialog();
                }
            }
            else
            {
                // Проверяем, заблокирован ли пользователь
                if (IsUserBlocked(login))
        {
                    ErrorMessage.Text = "Вы заблокированы. Обратитесь к администратору.";
                }
        else
                {
                    ErrorMessage.Text = "Неправильный логин или пароль. Пожалуйста, проверьте данные.";
                }
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private bool HasUserChangedPassword(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT IsPasswordChanged FROM Users WHERE Login = @login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    var isPasswordChanged = command.ExecuteScalar();
                    return isPasswordChanged != null && (bool)isPasswordChanged; // Проверяем статус смены пароля
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


        private bool AuthenticateUser(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Role, LastLogin, IsBlocked FROM Users WHERE Login = @login AND Password = @password";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["Role"].ToString();
                            bool isBlocked = (bool)reader["IsBlocked"];
                            DateTime? lastLogin = reader["LastLogin"] as DateTime?;

                            if (isBlocked)
                            {
                                return false; // Пользователь заблокирован
                            }

                            // Проверка, была ли последняя авторизация более месяца назад
                            if (lastLogin.HasValue && (DateTime.Now - lastLogin.Value).TotalDays > 30)
                            {
                                BlockUser(login); // Блокируем пользователя
                                return false; // Пользователь заблокирован из-за неактивности
                            }

                            // Обновляем дату последней авторизации
                            UpdateLastLogin(login);
                            return true; // Пользователь успешно авторизован
                        }
                        return false; // Неверный логин или пароль
                    }
                }
            }
        }



        private void UpdateLastLogin(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE Users SET LastLogin = @lastLogin WHERE Login = @login";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@lastLogin", DateTime.Now);
                    updateCommand.Parameters.AddWithValue("@login", login);
                    updateCommand.ExecuteNonQuery();
                }
            }
        }

        private void BlockUser(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string blockUserQuery = "UPDATE Users SET IsBlocked = 1 WHERE Login = @login"; // Обновляем статус блокировки
                using (SqlCommand blockUserCommand = new SqlCommand(blockUserQuery, connection))
        {
                    blockUserCommand.Parameters.AddWithValue("@login", login);
                    blockUserCommand.ExecuteNonQuery();
                }
            }
        }

    }
}