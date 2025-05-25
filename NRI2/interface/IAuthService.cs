using NRI.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public interface IAuthService
    {
        Task<bool> HasTwoFactorEnabledAsync(string login);
        Task<bool> ConfirmAccountAsync(string login, string confirmationCode);
        Task SendConfirmationCodeAsync(string login);
        Task<LoginResult> LoginAsync(string login, string password);
        Task<LoginResult> RegisterAsync(string login, string password, string secretKey);
        Task UpdateBlockStatusAsync(string login);
        Task<bool> IsUserBlockedAsync(string login);
        Task<int> GetIncorrectAttemptsAsync(string login);
        Task ResetIncorrectAttemptsAsync(string login);
        Task<bool> IsPasswordConfirmationRequiredAsync(string login);
        Task IncrementIncorrectAttemptsAsync(string login);
        Task BlockUserAsync(string login);
        Task<AuthResult> RegisterNewUserAsync(string login, string password, string fullName, string phone);
        Task<DataTable> GetUserAsync(string login);
        Task<bool> IsPasswordConfirmedAsync(string login);
        Task ShowNotificationAsync(string message, bool isError);
        Task<bool> VerifyTwoFactorCodeAsync(string login, string code);
        Task<bool> ChangePasswordAsync(string login, string newPasswordHash);
        Task<bool> ConfirmAccountWithTwoFactorAsync(string login, string code);
        Task<bool> SaveTwoFactorSecretAsync(string login, string secretKey);
        User GetCurrentUser();
        bool IsUserAuthenticated();
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public bool IsTwoFactorRequired { get; set; }
        public string SecretKey { get; set; }
        public string ErrorMessage { get; set; }
        public DataTable User { get; set; }

        public static LoginResult Success(DataTable user) => new() { IsSuccess = true, User = user };
        public static LoginResult TwoFactorRequired(string secretKey) => new() { IsTwoFactorRequired = true, SecretKey = secretKey };
        public static LoginResult Failed(string error) => new() { ErrorMessage = error };
    }
}
