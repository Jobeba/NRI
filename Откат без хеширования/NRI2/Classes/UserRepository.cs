using NRI;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MongoDB.Driver.Core.Configuration;
using NLog;


public class UserRepository : IUserRepository
{
    private readonly IDatabaseService _databaseService;

    public UserRepository(IDatabaseService databaseService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
    }

    public async Task<DataTable> GetUserByLoginAsync(string login)
    {
        var query = "SELECT * FROM Users WHERE login = @Login";
        var parameters = new[]
        {
        new SqlParameter("@Login", login)
    };

        return await _databaseService.ExecuteQueryAsync(query, parameters);
    }

    public async Task UpdateBlockStatusAsync(string login)
    {
        var query = "UPDATE Users SET user_blocked = dbo.Day_Block(date_auto, user_blocked) WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        await _databaseService.ExecuteNonQueryAsync(query, parameters);
    }

    public async Task<bool> IsUserBlockedAsync(string login)
    {
        var query = "SELECT user_blocked FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        var result = await _databaseService.ExecuteScalarAsync(query, parameters);
        return Convert.ToBoolean(result);
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
        var query = "UPDATE Users SET Incorrect_pass = 0 WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        await _databaseService.ExecuteNonQueryAsync(query, parameters);
    }

    public async Task<bool> IsPasswordConfirmationRequiredAsync(string login)
    {
        var query = "SELECT password_confirm FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        var result = await _databaseService.ExecuteScalarAsync(query, parameters);
        return Convert.ToBoolean(result);
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
