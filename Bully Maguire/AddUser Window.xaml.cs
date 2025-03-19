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


namespace Bully_Maguire
{
    public partial class AddUserWindow : Window
    {
        private const string ConnectionString = @"Data Source=209-U\SQLEXPRESS209;Initial Catalog=Bully Maguire1;Integrated Security=True;Encrypt=False";

        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text; // Логин
            string password = PasswordBox.Password; // Получаем пароль из PasswordBox
            string role = RoleTextBox.Text; // Роль

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // Проверка существования пользователя
                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Login = @Login";
                using (SqlCommand checkCommand = new SqlCommand(checkUserQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Login", login);
                    int userExists = (int)checkCommand.ExecuteScalar();

                    if (userExists > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Если пользователь не существует, добавляем его
                string insertQuery = "INSERT INTO Users (Login, Password, Role) VALUES (@Login, @Password, @Role)";
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Login", login);
                    insertCommand.Parameters.AddWithValue("@Password", password); // Не забудьте хешировать пароль в реальном приложении!
                    insertCommand.Parameters.AddWithValue("@Role", role);
                    insertCommand.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true; // Закрываем окно и возвращаем результат
            Close();
        }


    }
}


