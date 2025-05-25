using NRI;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MongoDB.Driver.Core.Configuration;
using NLog;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using NRI.Classes;
using Microsoft.Extensions.Logging;

public class UserRepository : IUserRepository
{
    private readonly IConfigService _configService;
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<UserRepository> _logger;
    private readonly string _connectionString;

    public UserRepository(
          IDatabaseService databaseService,
          IConfigService configService,
          ILogger<UserRepository> logger)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _connectionString = _configService.GetConnectionString();

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured");
        }
    }

    public async Task<DataTable> GetUserByLoginAsync(string login)
    {
        try
        {
            return await _databaseService.ExecuteQueryAsync(
                "SELECT * FROM Users WHERE login = @Login",
                new[] { new SqlParameter("@Login", login) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения пользователя по логину: {login}");
            throw;
        }
    }

    public async Task AssignPlayerRoleAsync(int userId, SqlTransaction transaction = null)
    {
        const int playerRoleId = 1; // ID роли "Игрок"
        await AssignRoleAsync(userId, playerRoleId, transaction);
    }
    public async Task AssignRoleAsync(int userId, int roleId, SqlTransaction transaction = null)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();
                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserID = @UserId AND RoleID = @RoleId)
                    BEGIN
                        INSERT INTO UserRoles (UserID, RoleID) 
                        VALUES (@UserId, @RoleId)
                    END";
                await connection.ExecuteAsync(query, new
                {
                    UserId = userId,
                    RoleId = roleId
                }, transaction);
            }
        }
         catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка назначения роли {roleId} пользователю {userId}");
            throw;
        }
    }
    public async Task<int> RegisterUserAsync(string fullName, string phone, string email, string login, string passwordHash)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
               await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var insertQuery = @"
                    INSERT INTO Users (
                        Full_name, 
                        Number_telephone, 
                        email,
                        login, 
                        password, 
                        Date_Auto,
                        account_confirmed,
                        password_confirm,
                    )
                    OUTPUT INSERTED.UserID
                    VALUES (
                        @FullName, 
                        @PhoneNumber, 
                        @Email,
                        @Login, 
                        @Password, 
                        GETDATE(),
                        0, -- account_confirmed
                        0, -- password_confirm
                    )";

                        var userId = await connection.ExecuteScalarAsync<int>(insertQuery, new
                        {
                            FullName = fullName,
                            PhoneNumber = phone,
                            Email = email,
                            Login = login,
                            Password = passwordHash
                        }, transaction);

                        // 2. Назначение роли "Игрок" по умолчанию
                        await AssignPlayerRoleAsync(userId, transaction);
                        transaction.Commit();
                        return userId;
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
            _logger.LogError(ex, "Ошибка регистрации пользователя");
            throw;
        }
    }
    public async Task<bool> UserHasRoleAsync(int userId, int roleId)
    {
        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT COUNT(1) 
                    FROM UserRoles 
                    WHERE UserID = @UserId AND RoleID = @RoleId";

                return await connection.ExecuteScalarAsync<bool>(query, new
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при проверке роли {roleId} у пользователя {userId}");
            throw;
        }
    }

    public async Task<bool> IsPasswordConfirmedAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login cannot be null or empty", nameof(login));
        using (var connection = new SqlConnection(_connectionString))
        {
            const string query = "SELECT password_confirm FROM Users WHERE login = @Login";
            return await connection.ExecuteScalarAsync<bool>(query, new { Login = login });
        }
    }
    public async Task<int> GetUserIdByLoginAsync(string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            _logger.LogError("Empty login passed to GetUserIdByLoginAsync");
            throw new ArgumentException("Login cannot be null or whitespace", nameof(login));
        }
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogError("Connection string is not initialized");
            throw new InvalidOperationException("Database connection is not configured");
        }

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                const string query = @"
                SELECT UserID 
                FROM Users 
                WHERE login = @Login";
                var userId = await connection.ExecuteScalarAsync<int?>(query, new { Login = login.Trim() });
                if (!userId.HasValue)
                {
                    _logger.LogWarning($"User not found: {login}"); // Используем Warn для NLog
                    throw new KeyNotFoundException($"User with login '{login}' not found");
                }
                return userId.Value;
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, $"Database error while getting user ID for {login}");
            throw new Exception("Database operation failed", sqlEx); // Заменяем DataAccessException на стандартное Exception
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error getting user ID for {login}");
            throw;
        }
    }
    public async Task<bool> IsAccountConfirmedAsync(string login)
    {
        if (string.IsNullOrEmpty(_configService.GetConnectionString()))
        {
            _logger.LogError("Строка подключения не инициализирована");
            return false;
        }

        try
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                var query = @"
                SELECT COALESCE(account_confirmed, 0) 
                FROM Users 
                WHERE login = @Login";
                var result = await connection.ExecuteScalarAsync<bool?>(query, new { Login = login });
                return result ?? false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка проверки подтверждения аккаунта {login}");
            return false;
        }
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        try
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT r.RoleName 
                FROM UserRoles ur
                JOIN Roles r ON ur.RoleID = r.RoleID
                WHERE ur.UserID = @UserId";

                var roles = await connection.QueryAsync<string>(query, new { UserId = userId });
                return roles.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения ролей для пользователя {userId}");
            return new List<string> { "Игрок" }; // Возвращаем роль по умолчанию
        }
    }


    public async Task UpdateBlockStatusAsync(string login)
    {
        var query = "UPDATE Users SET user_blocked = dbo.Day_Block(date_auto, user_blocked) WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        await _databaseService.ExecuteNonQueryAsync(query, parameters);
    }

    public async Task<bool> UserExistsAsync(string login)
    {
        try
        {
            var result = await _databaseService.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM Users WHERE login = @Login",
                new [] { new SqlParameter("@Login", login) });

            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки существования пользователя");
            throw;
        }
    }

    public async Task<bool> IsUserBlockedAsync(string login)
    {
        try
        {
            var result = await _databaseService.ExecuteScalarAsync(
                "SELECT user_blocked FROM Users WHERE login = @Login",
                new[] { new SqlParameter("@Login", login) });

            return result != null && Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка проверки блокировки пользователя: {login}");
            throw;
        }
    }

    public async Task<int> GetIncorrectAttemptsAsync(string login)
    {
        var query = "SELECT Incorrect_pass FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        var result = await _databaseService.ExecuteScalarAsync(query, parameters);
        return Convert.ToInt32(result);
    }
    public async Task ResetIncorrectAttemptsAsync(string login)
    {
        try
        {
            await _databaseService.ExecuteNonQueryAsync(
                "UPDATE Users SET Incorrect_pass = 0 WHERE login = @Login",
                new[] { new SqlParameter("@Login", login) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка сброса попыток входа для: {login}");
            throw;
        }
    }

    public async Task<bool> IsPasswordConfirmationRequiredAsync(string login)
    {
        var query = "SELECT password_confirm FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        var result = await _databaseService.ExecuteScalarAsync(query, parameters);
        return Convert.ToBoolean(result);
    }
    public async Task<bool> ChangePasswordAsync(string login, string newPasswordHash)
    {
        try
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                await connection.OpenAsync();
                string query = @"UPDATE Users 
                           SET password = @Password, 
                               Incorrect_pass = 0,
                               last_password_change = GETDATE()
                           WHERE login = @Login";
                int affected = await connection.ExecuteAsync(query, new
                {
                    Password = newPasswordHash,
                    Login = login
                });
                return affected > 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка смены пароля");
            throw;
        }
    }
    public async Task<int> GetUserRoleIdAsync(int userId)
    {
        try
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                string query = @"
                SELECT TOP 1 RoleID 
                FROM UserRoles 
                WHERE UserID = @UserId";

                return await connection.ExecuteScalarAsync<int>(query, new { UserId = userId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения роли для пользователя {userId}");
            return 1; // Возвращаем "Игрок" по умолчанию в случае ошибки
        }
    }

    public async Task<bool> IsTwoFactorEnabledAsync(string login)
    {
        try
        {
            using (var connection = new SqlConnection(_configService.GetConnectionString()))
            {
                await connection.OpenAsync();
                string query = @"SELECT CASE WHEN password_confirm = 1 
                           AND TwoFactorSecret IS NOT NULL 
                           THEN 1 ELSE 0 END 
                           FROM Users WHERE login = @Login";
                return await connection.ExecuteScalarAsync<bool>(query, new { Login = login });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки статуса 2FA");
            throw;
        }
    }

    public async Task IncrementIncorrectAttemptsAsync(string login)

    {

        var query = "UPDATE Users SET Incorrect_pass = Incorrect_pass + 1 WHERE login = @Login";

        var parameters = new[] { new SqlParameter("@Login", login) };

        await _databaseService.ExecuteNonQueryAsync(query, parameters);

    }
    public async Task ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)

    {

        await _databaseService.ExecuteNonQueryAsync(query, parameters);

    }

}
