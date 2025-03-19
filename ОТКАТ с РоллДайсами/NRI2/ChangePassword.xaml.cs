using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NRI
{
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        private void TypewriteTextblock(string textToAnimate, TextBlock txt, TimeSpan timeSpan)
        {
            var story = new Storyboard
            {
                FillBehavior = FillBehavior.HoldEnd,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var stringAnimation = new StringAnimationUsingKeyFrames
            {
                Duration = new Duration(timeSpan)
            };

            string tmp = string.Empty;
            foreach (char c in textToAnimate)
            {
                var discreteKeyFrame = new DiscreteStringKeyFrame
                {
                    KeyTime = KeyTime.Paced,
                    Value = tmp += c
                };
                stringAnimation.KeyFrames.Add(discreteKeyFrame);
            }

            Storyboard.SetTargetName(stringAnimation, txt.Name);
            Storyboard.SetTargetProperty(stringAnimation, new PropertyPath(TextBlock.TextProperty));
            story.Children.Add(stringAnimation);

            story.Begin(txt);
        }

        private void ChangePassword_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OldPassword.Text))
            {
                MessageBox.Show("Введите текущий пароль");
                return;
            }

            if (string.IsNullOrEmpty(NewPassword.Text))
            {
                MessageBox.Show("Введите новый пароль");
                return;
            }

            if (string.IsNullOrEmpty(ConfirmNewPassword.Text))
            {
                MessageBox.Show("Введите подтверждение нового пароля");
                return;
            }

            if (NewPassword.Text != ConfirmNewPassword.Text)
            {
                MessageBox.Show("Введенные пароли не совпадают");
                return;
            }

            try
            {
                using (var connection = new SqlConnection(@"Data Source=DESKTOP-FLP8EG6\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False"))
                {
                    connection.Open();

                    // Проверка старого пароля
                    var checkQuery = "SELECT COUNT(*) FROM Users WHERE login = @login AND password = @oldPassword";
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@login", Globals.Login);
                        checkCommand.Parameters.AddWithValue("@oldPassword", OldPassword.Text);

                        int count = (int)checkCommand.ExecuteScalar();
                        if (count == 0)
                        {
                            MessageBox.Show("Введен неправильный текущий пароль");
                            return;
                        }
                    }

                    // Обновление пароля
                    var updateQuery = "UPDATE Users SET password = @newPassword, password_confirm = 'True' WHERE login = @login";
                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@newPassword", NewPassword.Text);
                        updateCommand.Parameters.AddWithValue("@login", Globals.Login);

                        updateCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Пароль успешно изменен");
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}