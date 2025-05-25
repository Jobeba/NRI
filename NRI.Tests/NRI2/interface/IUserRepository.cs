using NRI;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using NLog;
using System.Collections.Generic;

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
    Task<bool> UserExistsAsync(string login);
    Task AssignPlayerRoleAsync(int userId, SqlTransaction transaction = null);
    Task AssignRoleAsync(int userId, int roleId, SqlTransaction transaction = null);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task<bool> UserHasRoleAsync(int userId, int roleId);
    Task<int> GetUserIdByLoginAsync(string login);
    Task<int> RegisterUserAsync(string fullName, string phone, string email, string login, string passwordHash);
    Task<bool> ChangePasswordAsync(string login, string newPasswordHash);
    Task<bool> IsAccountConfirmedAsync(string login);
    Task<bool> IsTwoFactorEnabledAsync(string login);
    Task<int> GetUserRoleIdAsync(int userId);
}
