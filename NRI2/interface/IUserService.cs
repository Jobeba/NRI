
using NRI.Classes;
using NRI.Shared;
using System;
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
        Task<List<User>> GetAllUserListAsync();
        Task UpdateUserActivityAsync(int userId);
        int GetCurrentUserId();
        Task<List<UserStatusDto>> GetOnlineUsersAsync();
        Task<UserStatisticsDto> GetUserStatisticsAsync();
    }
}
