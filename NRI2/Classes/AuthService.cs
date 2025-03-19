using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
        public async Task<LoginResult> LoginAsync(string login, string password)
        {
            try
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
    }
}