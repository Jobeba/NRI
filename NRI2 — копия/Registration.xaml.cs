using NRI;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics.Eventing.Reader;

namespace NRI.NRI
{
    /// <summary>
    /// Логика взаимодействия для Proj.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }
        private void  Hyperlink_click(object sender, RoutedEventArgs e)
        {
            Registrasya hyper = new Registrasya();
            hyper.Show();
            this.Close();
        }
        private void Registrastion_button_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBox.Text.Length > 0) // проверяем введён ли логин     
            {
                if (Passbox.Text.Length > 0) // проверяем введён ли пароль         
                {
                    DateBase database = new DateBase();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    DataTable table = new DataTable();
                    string query = $"select * from Users where login = '{LoginBox.Text}' and password = '{Passbox.Text}'";
                    SqlCommand command = new SqlCommand(query, database.Get());
                    adapter.SelectCommand = command;
                        adapter.Fill(table);
                    SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-FLP8EG6\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False");
                    sqlConnection.Open();
                    if (table.Rows.Count > 0) // если такая запись существует       
                    {
                        Globals.Sql = $"UPDATE Users SET user_blocked = dbo.Day_Block(date_auto, user_blocked) Where login = '{LoginBox.Text}'";
                        SqlCommand command_30day = new SqlCommand(Globals.Sql, sqlConnection);
                        Globals.date = Convert.ToBoolean(command_30day.ExecuteScalar());
                        Globals.Login = LoginBox.Text;
                        Globals.Sql = $"SELECT user_blocked FROM Users Where login = '{Globals.Login}'";
                        SqlCommand command_30day_1 = new SqlCommand(Globals.Sql, sqlConnection);
                        string dayblock = Convert.ToString(command_30day_1.ExecuteScalar());


                        if (dayblock == "True")
                        {
                            MessageBox.Show("Прошло 30 дней, ваша учётная запись просрочена. Обратитесь к администратору");
                            this.Close();
                        }
                        else
                        {
                            Globals.Sql = $"select Incorrect_pass from Kasyanov_NRI.dbo.Users Where login = '{LoginBox.Text}'";
                            SqlCommand command1 = new SqlCommand(Globals.Sql, sqlConnection);
                            Globals.Incorrect = Convert.ToInt32(command1.ExecuteScalar());
                            if (Globals.Incorrect > 3)
                            {
                                MessageBox.Show("Ваша учетная запись заблокирована. Обратитесь к администратору системы");
                                this.Close();
                            }
                            else
                            {
                                App.Current.Resources["CurrentUser"] = Passbox.Text;
                                MessageBox.Show("Вы успешно авторизовались"); // говорим, что авторизовался
                                Globals.Sql = $"update Users set Incorrect_pass = '0' Where login = '{LoginBox.Text}'";
                                SqlCommand command2 = new SqlCommand(Globals.Sql, sqlConnection);
                                Globals.Incorrect = Convert.ToInt32(command2.ExecuteScalar());
                                string login = LoginBox.Text;
                                Globals.Sql = $"select password_confirm from Users Where login = '{login}'";
                                SqlCommand command3 = new SqlCommand(Globals.Sql, sqlConnection);
                                string password_confirm = Convert.ToString(command3.ExecuteScalar());
                                if (password_confirm == "False")
                                {
                                    Globals.Login = login;
                                    MessageBox.Show("Замените пароль при первом входе");
                                    ChangePassword conferm = new ChangePassword();
                                    conferm.Show();
                                    this.Close();
                                }
                                else
                                {
                                    MainWindow main = new MainWindow();
                                    main.Show();
                                    this.Close();
                                }
                            }
                        }



                    }
                    else MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные"); // выводим ошибку  
                    if (table.Rows.Count <= 0) // если такая запись НЕ существует    
                    {
                        Globals.Sql = $"select Incorrect_pass from Kasyanov_NRI.dbo.Users Where login = '{LoginBox.Text}'";
                        SqlCommand command4 = new SqlCommand(Globals.Sql, sqlConnection);
                        Globals.Incorrect = Convert.ToInt32(command4.ExecuteScalar());
                        Globals.Incorrect++;
                        Globals.Sql = $"update Users set Incorrect_pass = '{Globals.Incorrect}' Where login = '{LoginBox.Text}'";
                        SqlCommand command5 = new SqlCommand(Globals.Sql, sqlConnection);
                        Convert.ToString(command5.ExecuteScalar());
                        if (Globals.Incorrect > 3)
                        {
                            MessageBox.Show("Вы заблокированы. Обратитесь к администратору");
                            this.Close();
                        }
                    }

                }
                else MessageBox.Show("Введите пароль"); // выводим ошибку    
            }
            else MessageBox.Show("Введите логин"); // выводим ошибку
        }

 

        private void Registrasya_click(object sender, RoutedEventArgs e)
        {
            Registrasya reg = new Registrasya();
            reg.Show();
            this.Close();
        }
    }
}
            
