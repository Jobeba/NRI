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


namespace Bully_Maguire
{
    public partial class EditUserWindow : Window
    {
        private string _login;
        private const string ConnectionString = @"Data Source=209-U\SQLEXPRESS209;Initial Catalog=Bully Maguire1;Integrated Security=True;Encrypt=False";

        public EditUserWindow(string login, string role)
    {
        InitializeComponent();
        _login = login;
        LoginTextBox.Text = login; // Теперь можно редактировать логин
        RoleTextBox.Text = role;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string newLogin = LoginTextBox.Text;
        string newRole = RoleTextBox.Text;

        using (SqlConnection connection = new SqlConnection(ConnectionString))
        {
            connection.Open();

            // Обновляем логин и роль
            string updateQuery = "UPDATE Users SET Login = @newLogin, Role = @role WHERE Login = @oldLogin";
            using (SqlCommand command = new SqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@newLogin", newLogin);
                command.Parameters.AddWithValue("@role", newRole);
                command.Parameters.AddWithValue("@oldLogin", _login);
                command.ExecuteNonQuery();
            }
        }

        MessageBox.Show("Изменения успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true; // Закрываем окно и возвращаем результат
        Close();
    }
}
}
