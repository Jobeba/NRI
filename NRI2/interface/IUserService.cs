using NRI.Classes;
using NRI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);

        // Добавляем новые методы
        Task<List<User>> GetAllUserListAsync();
        Task<List<User>> GetOnlineUsersAsync();
        Task UpdateUserActivityAsync(int userId);
        int GetCurrentUserId();
    }
}