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
using System.Data;
using System.Data.SqlClient;

namespace Bully_Maguire
{
    public partial class AdminWindow : Window
    {
        private const string ConnectionString = @"Data Source=209-U\SQLEXPRESS209;Initial Catalog=Bully Maguire1;Integrated Security=True;Encrypt=False";

        public AdminWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Login, Role, LastLogin, IsBlocked FROM Users"; // Включаем IsBlocked
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable usersTable = new DataTable();
                adapter.Fill(usersTable);
                UsersDataGrid.ItemsSource = usersTable.DefaultView;
            }
        }
                    private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is DataRowView selectedRow)
            {
                bool isBlocked = (bool)selectedRow["IsBlocked"];
                UserStatusTextBlock.Text = isBlocked ? "Статус: Заблокирован" : "Статус: Активен";
            }
            else
            {
                UserStatusTextBlock.Text = "Статус: ";
            }
        }

        private void AddUser_Button_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            bool? result = addUserWindow.ShowDialog(); // Открываем новое окно как модальное

            if (result == true)
            {
                LoadUsers(); // Обновляем список пользователей после добавления
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.Tag as DataRowView;

            if (user != null)
            {
                string login = user["Login"].ToString();
                string role = user["Role"].ToString();

                // Создаем и открываем окно редактирования пользователя
                EditUserWindow editUserWindow = new EditUserWindow(login, role);
                bool? result = editUserWindow.ShowDialog(); // Открываем новое окно как модальное

                if (result == true)
                {
                    LoadUsers(); // Обновляем список пользователей после редактирования
                }
            }
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var user = button?.Tag as DataRowView;

            if (user != null)
            {
                // Логика удаления пользователя
                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {user["Login"]}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление пользователя из базы данных
                    DeleteUser(user["Login"].ToString());
                    LoadUsers(); // Обновляем список пользователей
                }
            }
        }

        private void DeleteUser(string login)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM Users WHERE Login = @Login";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ChangeStatus_Button_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is DataRowView selectedRow)
            {
                bool isBlocked = (bool)selectedRow["IsBlocked"];
                string login = selectedRow["Login"].ToString();

                // Изменяем статус на противоположный
                bool newStatus = !isBlocked;

                // Обновляем базу данных
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE Users SET IsBlocked = @IsBlocked WHERE Login = @Login";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsBlocked", newStatus);
                        command.Parameters.AddWithValue("@Login", login);
                        command.ExecuteNonQuery();
                    }
                }

                // Обновляем отображение
                LoadUsers();
                UserStatusTextBlock.Text = newStatus ? "Статус: Заблокирован" : "Статус: Активен";
                MessageBox.Show($"Статус пользователя '{login}' изменен на {(newStatus ? "заблокирован" : "активен")}.");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для изменения статуса.");
            }
        }



    }
}



