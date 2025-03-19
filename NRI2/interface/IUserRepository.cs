using NRI;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using NLog;

public interface IUserRepository
{
    Task<DataTable> GetUserByLoginAsync(string login);
    Task UpdateBlockStatusAsync(string login);
    Task<bool> IsUserBlockedAsync(string login);
    Task<int> GetIncorrectAttemptsAsync(string login);
    Task ResetIncorrectAttemptsAsync(string login);
    Task<bool> IsPasswordConfirmationRequiredAsync(string login);
    Task IncrementIncorrectAttemptsAsync(string login);
    Task ExecuteNonQueryAsync(string query, params SqlParameter[] parameters);
}
