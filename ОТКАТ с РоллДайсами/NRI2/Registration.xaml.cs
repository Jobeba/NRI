using NLog;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NRI
{
    public partial class Registration : Window
    {
        private readonly IDatabaseService _databaseService;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IUserRepository _userRepository;



        private async Task NavigateToWindowAsync(Window window)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Show();
                    this.Close();
                });
            });
        }
        public Registration(IDatabaseService databaseService, IUserRepository userRepository)
        {
            InitializeComponent();
            _databaseService = databaseService;
            _userRepository = userRepository;
        }

        public Registration()
        {
        }

        private void NavigateToWindow(Window window)
        {
            window.Show();
            this.Close();
        }

        private async void Add_click(object sender, RoutedEventArgs e)
        {
            await NavigateToWindowAsync(new Projects());
        }
        private void Hyperlink_click(object sender, RoutedEventArgs e) => NavigateToWindow(new Registrasya());

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(LoginBox.Text) || LoginBox.Text.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа");
                return false;
            }

            if (string.IsNullOrEmpty(Passbox.Text) || Passbox.Text.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов");
                return false;
            }

            return true;
        }
        private async void Registrastion_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Info("Попытка авторизации пользователя: {0}", LoginBox.Text);
                if (!ValidateInput())
            {
                return;
            }
            if (string.IsNullOrEmpty(LoginBox.Text))
            {
                MessageBox.Show("Введите логин");
                return;
            }

            if (string.IsNullOrEmpty(Passbox.Text))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            try
            {
                using (var connection = new SqlConnection(@"Data Source=DESKTOP-FLP8EG6\MSSQLSERVER209;Initial Catalog=Kasyanov_NRI;Integrated Security=True;Encrypt=False"))
                {
                    await connection.OpenAsync();

                    // Проверка логина и пароля
                    var query = "SELECT * FROM Users WHERE login = @Login AND password = @Password";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", LoginBox.Text);
                        command.Parameters.AddWithValue("@Password", Passbox.Text);

                        var adapter = new SqlDataAdapter(command);
                        var table = new DataTable();
                        adapter.Fill(table);

                        if (table.Rows.Count > 0)
                        {
                            await HandleSuccessfulLogin(connection);
                        }
                        else
                        {
                            await HandleFailedLogin(connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}");
                LogError(ex);
            }
                Logger.Info("Пользователь успешно авторизован: {0}", LoginBox.Text);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при авторизации пользователя: {0}", LoginBox.Text);
                MessageBox.Show("Произошла ошибка при авторизации. Подробности в логах.");
            }
        }

        private async Task HandleSuccessfulLogin(SqlConnection connection)
        {
            // Обновление статуса блокировки
            var updateBlockStatusQuery = "UPDATE Users SET user_blocked = dbo.Day_Block(date_auto, user_blocked) WHERE login = @Login";
            using (var command = new SqlCommand(updateBlockStatusQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                await command.ExecuteNonQueryAsync();
            }

            // Проверка блокировки
            var checkBlockStatusQuery = "SELECT user_blocked FROM Users WHERE login = @Login";
            using (var command = new SqlCommand(checkBlockStatusQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                var isBlocked = Convert.ToBoolean(await command.ExecuteScalarAsync());

                if (isBlocked)
                {
                    MessageBox.Show("Прошло 30 дней, ваша учётная запись просрочена. Обратитесь к администратору");
                    this.Close();
                    return;
                }
            }

            // Проверка количества неверных попыток входа
            var incorrectAttemptsQuery = "SELECT Incorrect_pass FROM Users WHERE login = @Login";
            int incorrectAttempts;
            using (var command = new SqlCommand(incorrectAttemptsQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                incorrectAttempts = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            if (incorrectAttempts > 3)
            {
                MessageBox.Show("Ваша учетная запись заблокирована. Обратитесь к администратору системы");
                this.Close();
                return;
            }

            // Сброс счетчика неверных попыток
            var resetIncorrectAttemptsQuery = "UPDATE Users SET Incorrect_pass = 0 WHERE login = @Login";
            using (var command = new SqlCommand(resetIncorrectAttemptsQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                await command.ExecuteNonQueryAsync();
            }

            // Проверка необходимости смены пароля
            var checkPasswordConfirmQuery = "SELECT password_confirm FROM Users WHERE login = @Login";
            using (var command = new SqlCommand(checkPasswordConfirmQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                var passwordConfirm = Convert.ToBoolean(await command.ExecuteScalarAsync());

                if (!passwordConfirm)
                {
                    MessageBox.Show("Замените пароль при первом входе");
                    NavigateToWindow(new ChangePassword());
                }
                else
                {
                    MessageBox.Show("Вы успешно авторизовались");
                    NavigateToWindow(new MainWindow());
                }
            }

        }
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


        private async Task HandleFailedLogin(SqlConnection connection)
        {
            // Увеличение счетчика неверных попыток
            var incrementIncorrectAttemptsQuery = "UPDATE Users SET Incorrect_pass = Incorrect_pass + 1 WHERE login = @Login";
            using (var command = new SqlCommand(incrementIncorrectAttemptsQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                await command.ExecuteNonQueryAsync();
            }

            // Проверка блокировки
            var checkIncorrectAttemptsQuery = "SELECT Incorrect_pass FROM Users WHERE login = @Login";
            int incorrectAttempts;
            using (var command = new SqlCommand(checkIncorrectAttemptsQuery, connection))
            {
                command.Parameters.AddWithValue("@Login", LoginBox.Text);
                incorrectAttempts = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            if (incorrectAttempts > 3)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору");
                this.Close();
            }
            else
            {
                MessageBox.Show("Вы ввели неверный логин или пароль. Пожалуйста, проверьте ещё раз введенные данные");
            }
        }
        public async Task<DataTable> GetUserByLoginAsync(string login, string password)
        {
            var query = "SELECT * FROM Users WHERE login = @Login";
            var parameters = new[] { new SqlParameter("@Login", login) };
            var user = await _databaseService.ExecuteQueryAsync(query, parameters);

            if (user.Rows.Count > 0)
            {
                var hashedPassword = user.Rows[0]["password"].ToString();
                if (VerifyPassword(password, hashedPassword))
                {
                    return user;
                }
            }

            return new DataTable();
        }

        private void LogError(Exception ex)
        {
            // Логирование ошибок в файл или базу данных
            string logMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";
            System.IO.File.AppendAllText("error_log.txt", logMessage);
        }
    }
}