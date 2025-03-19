using NRI;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System;

public class UserRepository : IUserRepository
{
    private readonly IDatabaseService _databaseService;

    public UserRepository(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<DataTable> GetUserByLoginAsync(string login, string password)
    {
        var query = "SELECT * FROM Users WHERE login = @Login AND password = @Password";
        var parameters = new[]
        {
            new SqlParameter("@Login", login),
            new SqlParameter("@Password", password)
        };
        return await _databaseService.ExecuteQueryAsync(query, parameters);
    }

    public async Task UpdateUserBlockStatusAsync(string login)
    {
        var query = "UPDATE Users SET user_blocked = dbo.Day_Block(date_auto, user_blocked) WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        await _databaseService.ExecuteNonQueryAsync(query, parameters);
    }

    public async Task<int> GetIncorrectAttemptsAsync(string login)
    {
        var query = "SELECT Incorrect_pass FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        return Convert.ToInt32(await _databaseService.ExecuteScalarAsync(query, parameters));
    }

    public async Task ResetIncorrectAttemptsAsync(string login)
    {
        var query = "UPDATE Users SET Incorrect_pass = 0 WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        await _databaseService.ExecuteNonQueryAsync(query, parameters);
    }

    public async Task<bool> IsPasswordConfirmedAsync(string login)
    {
        var query = "SELECT password_confirm FROM Users WHERE login = @Login";
        var parameters = new[] { new SqlParameter("@Login", login) };
        return Convert.ToBoolean(await _databaseService.ExecuteScalarAsync(query, parameters));
    }
}