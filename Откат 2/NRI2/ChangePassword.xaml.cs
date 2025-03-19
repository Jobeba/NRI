using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
        }
        private void TypewriteTextblock(string textToAnimate, TextBlock txt, TimeSpan timeSpan)
        {
            Storyboard story = new Storyboard();
            story.FillBehavior = FillBehavior.HoldEnd;
            story.RepeatBehavior = RepeatBehavior.Forever;

            DiscreteStringKeyFrame discreteStringKeyFrame;
            StringAnimationUsingKeyFrames stringAnimationUsingKeyFrames = new StringAnimationUsingKeyFrames();
            stringAnimationUsingKeyFrames.Duration = new Duration(timeSpan);

            string tmp = string.Empty;
            foreach (char c in textToAnimate)
            {
                discreteStringKeyFrame = new DiscreteStringKeyFrame();
                discreteStringKeyFrame.KeyTime = KeyTime.Paced;
                tmp += c;
                discreteStringKeyFrame.Value = tmp;
                stringAnimationUsingKeyFrames.KeyFrames.Add(discreteStringKeyFrame);
            }
            Storyboard.SetTargetName(stringAnimationUsingKeyFrames, txt.Name);
            Storyboard.SetTargetProperty(stringAnimationUsingKeyFrames, new PropertyPath(TextBlock.TextProperty));
            story.Children.Add(stringAnimationUsingKeyFrames);

            story.Begin(txt);
        }

        private void ChangePassword_button_Click(object sender, RoutedEventArgs e)
        {
            if (OldPassword.Text.Length > 0)
            {
                if (NewPassword.Text.Length > 0)
                {
                    if (ConfirmNewPassword.Text.Length > 0)
                    {
                        if (NewPassword.Text != ConfirmNewPassword.Text)
                        {
                            MessageBox.Show("Введенные пароли не совпадают");
                        }
                        if (NewPassword.Text == ConfirmNewPassword.Text)
                        {
                            DateBase database = new DateBase();
                            SqlDataAdapter adapter = new SqlDataAdapter();
                            DataTable table = new DataTable();
                            string login = Globals.Login;
                            string query = $"select * from Users where login = '{login}' and password = '{OldPassword.Text}'";
                            SqlCommand command = new SqlCommand(query, database.Get());
                            adapter.SelectCommand = command;
                            adapter.Fill(table);
                            if (table.Rows.Count > 0) // если такая запись существует 
                            {
                                string password = NewPassword.Text;
                                string password_confirm = "False";
                                SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-FLP8EG6\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False");
                                
                                sqlConnection.Open();
                                string sql = $"update Users set password = '{password}' , password_confirm = 'True' WHERE login = '{login}'";
                                SqlCommand command1 = new SqlCommand(sql, sqlConnection);
                                string Confern = Convert.ToString(command1.ExecuteScalar());
                                MessageBox.Show("Вы успешно авторизовались");
                                MainWindow main = new MainWindow();
                                main.Show();
                                this.Close();

                            }
                            else
                            {
                                MessageBox.Show("Введен неправильный текущий пароль");
                            }

                        }
                    }
                    else MessageBox.Show("Введите подтверждение нового пароля");

                }
                else MessageBox.Show("Введите новый пароль");
            }
            else MessageBox.Show("Введите текущий пароль");

        }

    }
}
    

