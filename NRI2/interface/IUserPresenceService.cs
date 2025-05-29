using Microsoft.EntityFrameworkCore;
using NRI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NRI.Services
{
    public interface IUserPresenceService
    {
        Task<HashSet<int>> GetOnlineUsersAsync();
        Task<Dictionary<int, DateTime?>> GetUserActivityAsync();
        Task UpdateUserPresence(int userId);
    }

    public class UserPresenceService : IUserPresenceService
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public UserPresenceService(DbContextOptions<AppDbContext> dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        public async Task<Dictionary<int, DateTime?>> GetUserActivityAsync()
        {
            using var context = new AppDbContext(_dbContextOptions);
            return await context.Users
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => u.LastActivity);
        }

        public async Task<HashSet<int>> GetOnlineUsersAsync()
        {
            using var context = new AppDbContext(_dbContextOptions);
            var threshold = DateTime.UtcNow.AddMinutes(-5);
            var onlineUsers = await context.Users
                .AsNoTracking()
                .Where(u => u.LastActivity >= threshold)
                .Select(u => u.Id)
                .ToListAsync();
            return new HashSet<int>(onlineUsers);
        }

        public async Task UpdateUserPresence(int userId)
        {
            using var context = new AppDbContext(_dbContextOptions);
            var user = await context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastActivity = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
    }
}
