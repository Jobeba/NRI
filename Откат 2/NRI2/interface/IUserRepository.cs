using System.Data;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<DataTable> GetUserByLoginAsync(string login, string password);
    Task UpdateUserBlockStatusAsync(string login);
    Task<int> GetIncorrectAttemptsAsync(string login);
    Task ResetIncorrectAttemptsAsync(string login);
    Task<bool> IsPasswordConfirmedAsync(string login);
}