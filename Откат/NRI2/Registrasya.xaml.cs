using System;
using System.Windows;
using System.Data.SqlClient;
using System.Data;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для Registrasya.xaml
    /// </summary>
    public partial class Registrasya : Window
    {
        public Registrasya()
        {
            InitializeComponent();
        }
        private void Registr_button_Click(object sender, RoutedEventArgs e)
        {
            {
                try
                {
                    DateBase database = new DateBase();
                    database.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DataTable table = new DataTable();

                    string queryNewUsers = $"insert into Users (Full_name, Number_telephone, login, password) " +
                                        $"values ( " +
                                        $"'{FullName_textbox.Text}', " +
                                        $"'{Number_telephone.Text}', " +
                                        $"'{Login_textbox.Text}', " +
                                        $"'{passwordBox.Password}' )";

                    string queryCheckForExistingUsers = $"SELECT * FROM Users WHERE login = '{Login_textbox.Text}'";
                    SqlCommand commandNewUsers = new SqlCommand(queryNewUsers, database.Get());
                    SqlCommand commandCheckForExistingUsers = new SqlCommand(queryCheckForExistingUsers, database.Get());

                    adapter.SelectCommand = commandCheckForExistingUsers;
                    adapter.Fill(table);

                    if (FullName_textbox.Text == "" || Number_telephone.Text == "" || Login_textbox.Text == "" || passwordBox.Password == "")
                    {
                        MessageBox.Show("Заполнены не все поля!");
                    }
                    else
                    {
                        if (table.Rows.Count > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует!");
                        }
                        else
                        {
                            commandNewUsers.ExecuteNonQuery();
                            MessageBox.Show("Пользователь успешно создан! Выполните авторизацию!");
                            Registration reg = new Registration();
                            reg.Show();
                            this.Close();
                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Registration reg = new Registration();
            reg.Show();
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            long totalMemory = GC.GetTotalMemory(false);

            base.OnClosed(e);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
        ~Registrasya()
        {
            InitializeComponent();
        }
    }
}
