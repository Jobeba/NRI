using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace DBNotary
{
    public partial class MainWindow : Window
    {
        private int failedAttempts = 0; // Счетчик неудачных попыток

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            string connectionString = "Data Source=DESKTOP-J8U6FKK\\SQLEXPRESS;Initial Catalog=NotaryDB;Integrated Security=True;Encrypt=False";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Проверка статуса блокировки и даты последнего входа
                    string query = "SELECT IsLocked, LastLogin FROM Users WHERE Username = @username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool isLocked = reader.GetBoolean(0);
                                DateTime? lastLogin = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);

                                if (isLocked)
                                {
                                    MessageBox.Show("Ваш аккаунт заблокирован. Пожалуйста, обратитесь в службу поддержки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                // Проверка на блокировку по времени
                                if (lastLogin.HasValue && (DateTime.Now - lastLogin.Value).TotalDays > 30)
                                {
                                    string lockQuery = "UPDATE Users SET IsLocked = 1 WHERE Username = @username";
                                    using (SqlCommand lockCmd = new SqlCommand(lockQuery, conn))
                                    {
                                        lockCmd.Parameters.AddWithValue("@username", username);
                                        lockCmd.ExecuteNonQuery();
                                    }

                                    MessageBox.Show("Ваш аккаунт заблокирован из-за отсутствия входа в систему более 30 дней.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    // Проверка пароля
                    query = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            // Обновление времени последнего входа
                            string updateLoginQuery = "UPDATE Users SET LastLogin = @lastLogin WHERE Username = @username";
                            using (SqlCommand updateCmd = new SqlCommand(updateLoginQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@lastLogin", DateTime.Now);
                                updateCmd.Parameters.AddWithValue("@username", username);
                                updateCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Успешный вход в систему DBNotary!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            failedAttempts = 0; // Сброс счетчика при успешном входе
                        }
                        else
                        {
                            failedAttempts++;
                            MessageBox.Show("Неправильное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                            if (failedAttempts >= 3)
                            {
                                string lockQuery = "UPDATE Users SET IsLocked = 1 WHERE Username = @username";
                                using (SqlCommand lockCmd = new SqlCommand(lockQuery, conn))
                                {
                                    lockCmd.Parameters.AddWithValue("@username", username);
                                    lockCmd.ExecuteNonQuery();
                                }

                                MessageBox.Show("Вы превысили количество попыток входа. Ваш аккаунт заблокирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}
