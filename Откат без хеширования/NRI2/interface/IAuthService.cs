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

        
        Task<LoginResult> LoginAsync(string login, string password);
        Task UpdateBlockStatusAsync(string login);
        Task<bool> IsUserBlockedAsync(string login);
        Task<int> GetIncorrectAttemptsAsync(string login);
        Task ResetIncorrectAttemptsAsync(string login);
        Task<bool> IsPasswordConfirmationRequiredAsync(string login);
        Task IncrementIncorrectAttemptsAsync(string login);
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public DataTable User { get; set; }
        public string ErrorMessage { get; set; }
    }
}
