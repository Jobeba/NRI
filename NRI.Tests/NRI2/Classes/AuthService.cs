using NLog;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using NRI.Models;
using OtpNet;
using System.Windows;
using NRI.Services;
using System.Security.Claims;

namespace NRI.Classes

{

    public class AuthService : IAuthService
    {
        private readonly JwtService _jwtService;
        private readonly Action<string, bool> _showNotification;
        private readonly IDatabaseService _database;
        private readonly IConfigService _configService;
        private readonly IUserRepository _userRepository;
        private readonly string _connectionString;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AuthService(IUserRepository userRepository,
                          IOptions<DatabaseSettings> databaseSettings,
                          IConfigService configService,
                          JwtService jwtService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _connectionString = databaseSettings.Value.DefaultConnection ?? throw new ArgumentNullException(nameof(databaseSettings));
            _configService = configService;
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }


        public bool IsUserAuthenticated()
        {
            if (Application.Current.Properties.Contains("JwtToken"))
            {
                var token = Application.Current.Properties["JwtToken"]?.ToString();
                return !string.IsNullOrEmpty(token);
            }
            return false;
        }

        public User GetCurrentUser()
        {
            if (Application.Current.Properties.Contains("JwtToken"))
            {
                var token = Application.Current.Properties["JwtToken"]?.ToString();
                var principal = _jwtService.ValidateToken(token);

                if (principal != null)
                {
                    return new User
                    {
                        Id = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                        Login = principal.FindFirst(ClaimTypes.Name)?.Value,
                        Token = token
                    };
                }
            }
            return null;
        }
        public async Task<bool> EnableTwoFactorAuthAsync(string login, string secretKey)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Устанавливаем ТОЛЬКО password_confirm и секретный ключ
                    var command = new SqlCommand(
                        "UPDATE Users SET TwoFactorSecret = @secretKey, password_confirm = 1 " +
                        "WHERE login = @login", connection);
                    command.Parameters.AddWithValue("@secretKey", secretKey);
                    command.Parameters.AddWithValue("@login", login);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при включении 2FA для пользователя {Login}", login);
                return false;
            }
        }

        public async Task<bool> ConfirmAccountWithTwoFactorAsync(string login, string code)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    // Получаем секретный ключ из базы
                    string secretKey = await connection.ExecuteScalarAsync<string>(
                        "SELECT TwoFactorSecret FROM Users WHERE login = @Login",
                        new { Login = login });

                    if (string.IsNullOrEmpty(secretKey))
                    {
                        Logger.Error("TwoFactorSecret не найден для пользователя {Login}", login);
                        return false;
                    }

                    // Проверяем код через Totp
                    var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                    if (totp.VerifyTotp(code, out _))
                    {
                        // Если код верный, подтверждаем аккаунт
                        await connection.ExecuteAsync(
                            "UPDATE Users SET password_confirm = 1 WHERE login = @Login",
                            new { Login = login });
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка подтверждения аккаунта через 2FA");
                throw;
            }
            return false;
        }

        public async Task<bool> SaveTwoFactorSecretAsync(string login, string secretKey)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    int affected = await connection.ExecuteAsync(
                        "UPDATE Users SET TwoFactorSecret = @SecretKey WHERE login = @Login",
                        new { SecretKey = secretKey, Login = login });

                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка сохранения TwoFactorSecret");
                throw;
            }
        }

        public async Task<bool> HasTwoFactorEnabledAsync(string login)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT CASE 
                             WHEN TwoFactorSecret IS NOT NULL AND password_confirm = 1 
                             THEN 1 ELSE 0 END 
                         FROM Users WHERE login = @Login";
                    return await connection.ExecuteScalarAsync<bool>(query, new { Login = login });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка проверки статуса 2FA");
                throw;
            }
        }


        public async Task<bool> ConfirmUserAccountAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentNullException(nameof(login));
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Начинаем явную транзакцию
                    using (var transaction = connection.BeginTransaction())
                   {
                        try
                        {
                            // 1. Проверяем существование пользователя
                            var existsQuery = "SELECT 1 FROM Users WHERE login = @Login";

                            var exists = await connection.ExecuteScalarAsync<bool?>(existsQuery,

                                new { Login = login }, transaction);



                            if (!exists.HasValue || !exists.Value)

                                return false;
                            // 2. Подтверждаем аккаунт с явным COMMIT

                            var updateQuery = @"

                                        UPDATE Users 

                                        SET account_confirmed = 1 

                                        WHERE login = @Login";



                            int affectedRows = await connection.ExecuteAsync(updateQuery,

                                new { Login = login }, transaction);



                            // Явный COMMIT

                            transaction.Commit();



                            return affectedRows > 0;

                        }

                        catch

                        {

                            transaction.Rollback();

                            throw;

                        }

                    }

                }

            }

            catch (Exception ex)

            {

                Logger.Error(ex, $"Error confirming account for {login}");

                return false;

            }

        }



        public async Task<bool> IsPasswordConfirmedAsync(string login)

        {

            using (var connection = new SqlConnection(_configService.GetConnectionString()))

            {

                await connection.OpenAsync();

                string query = "SELECT password_confirm FROM Users WHERE login = @Login";

                using (var cmd = new SqlCommand(query, connection))

                {

                    cmd.Parameters.AddWithValue("@Login", login);

                    var result = await cmd.ExecuteScalarAsync();

                    return result != null && (bool)result;

                }

            }

        }

        public async Task ShowNotificationAsync(string message, bool isError)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _showNotification?.Invoke(message, isError);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка при отображении уведомления");
            }
        }
        public async Task<bool> VerifyTwoFactorCodeAsync(string login, string code)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    // Получаем статус подтверждения 2FA и секретный ключ
                    var userInfo = await connection.QueryFirstOrDefaultAsync<dynamic>(
                        @"SELECT TwoFactorSecret, password_confirm 
                  FROM Users 
                  WHERE login = @Login",
                        new { Login = login });

                    if (userInfo == null)
                    {
                        Logger.Error("Пользователь {Login} не найден", login);
                        return false;
                    }

                    // Если ключ есть, но 2FA не подтверждена
                    if (!string.IsNullOrEmpty(userInfo.TwoFactorSecret) && !userInfo.password_confirm)
                    {
                        Logger.Warn("2FA не подтверждена для пользователя {Login}", login);
                        return false;
                    }

                    // Если ключ есть и 2FA подтверждена
                    if (userInfo.password_confirm && !string.IsNullOrEmpty(userInfo.TwoFactorSecret))
                    {
                        var secretBytes = Base32Encoding.ToBytes(userInfo.TwoFactorSecret);
                        var totp = new Totp(secretBytes);

                        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
                    }

                    Logger.Warn("2FA не настроена для пользователя {Login}", login);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка проверки 2FA кода для {Login}", login);
                throw;
            }
        }
        public async Task<bool> ChangePasswordAsync(string login, string newPasswordHash)
        {
            try
            {
                using (var connection = new SqlConnection(_configService.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    string query = @"
                UPDATE Users 
                SET password = @NewPassword, 
                    password_confirm = 0,
                    Incorrect_pass = 0,
                    Date_Auto = GETDATE()
                WHERE login = @Login";
                    await connection.ExecuteAsync(query, new
                    {
                        NewPassword = newPasswordHash,
                        Login = login
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка смены пароля");
                return false;
            }
        }

        public async Task<AuthResult> RegisterNewUserAsync(string login, string password, string fullName, string phone)
        {
            try
           {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // Проверка существующего пользователя
                    var userExists = await connection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM Users WHERE login = @Login",
                        new { Login = login });
                    if (userExists > 0)
                    {
                        return AuthResult.Failed("Пользователь с таким логином уже существует");
                    }
                    // Хеширование пароля
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    var secretKey = KeyGeneration.GenerateRandomKey(20);
                    var base32Secret = Base32Encoding.ToString(secretKey);
                    // Регистрация пользователя
                    var query = @"
                        INSERT INTO Users (Full_name, Number_telephone, login, password, Date_Auto, TwoFactorSecret)
                        VALUES (@FullName, @PhoneNumber, @Login, @Password, GETDATE(), @TwoFactorSecret)";
                   await connection.ExecuteAsync(query, new
                    {
                        FullName = fullName,
                        PhoneNumber = phone,
                        Login = login,
                        Password = hashedPassword,
                        TwoFactorSecret = base32Secret
                    });
                    // Назначение роли "Игрок" по умолчанию
                    await connection.ExecuteAsync(
                        "INSERT INTO UserRoles (UserID, RoleID) " +
                        "VALUES ((SELECT UserID FROM Users WHERE login = @Login), " +
                        "(SELECT RoleID FROM Roles WHERE RoleName = 'Игрок'))",
                        new { Login = login });
                    return AuthResult.Success(null);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Ошибка регистрации нового пользователя");
                return AuthResult.Failed("Ошибка регистрации");
            }
        }
        public async Task BlockUserAsync(string login)
        {
            var query = "UPDATE Users SET user_blocked = true WHERE login = @login";
            var parameters = new[] { new SqlParameter("@login", login) };
            await _database.ExecuteNonQueryAsync(query, parameters);
        }
        public async Task SendConfirmationCodeAsync(string login)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // 1. Проверяем, существует ли пользователь
                    var userExists = await _userRepository.UserExistsAsync(login);
                    if (!userExists)
                    {
                        Logger.Warn($"Попытка отправить код подтверждения несуществующему пользователю: {login}");
                        return;
                    }
                    // 2. Генерируем код подтверждения (например, 6-значный цифровой код)
                    var confirmationCode = new Random().Next(100000, 999999).ToString();
                    var expirationTime = DateTime.Now.AddMinutes(15);

                    // 3. Сохраняем код в базе данных
                    var query = @"UPDATE Users 
                             SET ConfirmationCode = @Code, 
                                 ConfirmationCodeExpiration = @Expiration
                             WHERE Login = @Login";
                    await connection.ExecuteAsync(query, new
                    {
                        Code = confirmationCode,
                        Expiration = expirationTime,
                        Login = login
                    });
                    // 4. Отправляем код пользователю (в реальном приложении здесь была бы отправка email/SMS)
                    Logger.Info($"Код подтверждения для {login}: {confirmationCode} (имитация отправки)");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Ошибка при отправке кода подтверждения для {login}");
                throw; // Можно обработать по-другому в зависимости от требований
            }
        }
        public async Task<bool> ConfirmAccountAsync(string login, string confirmationCode)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // 1. Проверяем, что код совпадает и аккаунт еще не подтвержден
                    bool isCodeValid = await connection.ExecuteScalarAsync<bool>(
                        @"SELECT COUNT(1) FROM Users 
                          WHERE login = @Login 
                          AND TwoFactorSecret = @Code",
                        new { Login = login, Code = confirmationCode });
                    if (!isCodeValid)
                    {
                        Logger.Warn($"Неверный код или аккаунт уже подтверждён: {login}");
                        return false;
                    }
                    // 2. Подтверждаем аккаунт (ОБНОВЛЯЕМ ТОЛЬКО account_confirmed)
                    int affectedRows = await connection.ExecuteAsync(
                        @"UPDATE Users 
                  SET account_confirmed = 1,
                      TwoFactorSecret = NULL
                  WHERE login = @Login",
                        new { Login = login });
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Ошибка подтверждения аккаунта для {login}");
                return false;
            }
        }
        public async Task<LoginResult> LoginAsync(string login, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    Logger.Info($"Попытка авторизации: {login}");
                    // Проверка ввода
                    if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                    {
                        Logger.Warn("Логин или пароль не могут быть пустыми");
                        return new LoginResult { IsSuccess = false, ErrorMessage = "Логин или пароль не могут быть пустыми" };
                    }
                    // Получение пользователя из репозитория
                    var user = await _userRepository.GetUserByLoginAsync(login);
                    if (user.Rows.Count > 0)
                    {
                        string storedHash = user.Rows[0]["password"].ToString();
                        // Логирование для отладки
                        Logger.Info($"Хэш пароля для пользователя {login}: {storedHash}");
                        // Проверка пароля с использованием BCrypt
                        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, storedHash);
                        if (isPasswordValid)
                        {
                            Logger.Info($"Пользователь {login} успешно авторизован");
                            return new LoginResult { IsSuccess = true, User = user };
                        }
                        else
                        {
                            Logger.Warn($"Неверный пароль для пользователя {login}");
                           return new LoginResult { IsSuccess = false, ErrorMessage = "Неверный логин или пароль" };
                        }
                    }
                    else
                    {
                        Logger.Warn($"Пользователь {login} не найден");
                        return new LoginResult { IsSuccess = false, ErrorMessage = "Неверный логин или пароль" };
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Ошибка при авторизации пользователя {login}");
                return new LoginResult { IsSuccess = false, ErrorMessage = "Ошибка сервера" };
            }
        }
        public async Task UpdateBlockStatusAsync(string login)
        {
            await _userRepository.UpdateBlockStatusAsync(login);
        }
        public async Task<bool> IsUserBlockedAsync(string login)
        {
            return await _userRepository.IsUserBlockedAsync(login);
        }
        public async Task<int> GetIncorrectAttemptsAsync(string login)
        {
            return await _userRepository.GetIncorrectAttemptsAsync(login);
        }
        public async Task ResetIncorrectAttemptsAsync(string login)
        {
            await _userRepository.ResetIncorrectAttemptsAsync(login);
        }
        public async Task<bool> IsPasswordConfirmationRequiredAsync(string login)
        {
            return await _userRepository.IsPasswordConfirmationRequiredAsync(login);
        }
        public async Task IncrementIncorrectAttemptsAsync(string login)
        {
            await _userRepository.IncrementIncorrectAttemptsAsync(login);
        }
        public async Task UpdateLastLoginDateAsync(string login)
        {
            var query = "UPDATE Users SET Date_Auto = GETDATE() WHERE login = @Login";
            var parameters = new[] { new SqlParameter("@Login", login) };
            await _userRepository.ExecuteNonQueryAsync(query, parameters);
        }
        public async Task<LoginResult> RegisterAsync(string login, string password, string secretKey)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var query = @"
                INSERT INTO Users (Login, PasswordHash, TwoFactorSecret)
                VALUES (@Login, @PasswordHash, @TwoFactorSecret)";
                await connection.ExecuteAsync(query, new
                {
                    Login = login,
                    PasswordHash = hashedPassword,
                    TwoFactorSecret = secretKey
                });
                return LoginResult.Success(null);

            }
        }
       
        public async Task<DataTable> GetUserAsync(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users WHERE Login = @Login";
                var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Login = login });
                var dataTable = new DataTable();
                dataTable.Columns.Add("Login", typeof(string));
                dataTable.Columns.Add("PasswordHash", typeof(string));
                dataTable.Columns.Add("TwoFactorSecret", typeof(string));
                dataTable.Columns.Add("IsBlocked", typeof(bool));
                dataTable.Columns.Add("IncorrectAttempts", typeof(int));

                if (user != null)
               {
                    dataTable.Rows.Add(
                        user.Login,
                        user.PasswordHash,

                        user.TwoFactorSecret,

                        user.IsBlocked,

                        user.IncorrectAttempts);

                }

                return dataTable;

            }

        }

    }

}