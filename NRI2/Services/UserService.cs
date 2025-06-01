// UserService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NRI.Classes;
using NRI.Data;
using NRI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NRI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _onlineThreshold = TimeSpan.FromMinutes(5);
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        public async Task UpdateUserActivityAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserStatusDto>> GetOnlineUsersAsync()
        {
            return await _context.Users
                .Where(u => u.LastActivity != null)
                .Select(u => new UserStatusDto
                {
                    UserId = u.Id,
                    Username = u.Login,
                    IsOnline = DateTime.UtcNow - u.LastActivity.Value < _onlineThreshold,
                    LastActivity = u.LastActivity.Value
                })
                .OrderByDescending(u => u.LastActivity)
                .ToListAsync();
        }

        public async Task<List<User>> GetAllUserListAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var now = DateTime.UtcNow;

            return new UserStatisticsDto
            {
                TotalUsers = users.Count,
                OnlineUsers = users.Count(u => u.LastActivity != null &&
                    now - u.LastActivity.Value < _onlineThreshold),
                OfflineUsers = users.Count(u => u.LastActivity == null ||
                    now - u.LastActivity.Value >= _onlineThreshold),
                LastUpdated = DateTime.Now
            };
        }
    }
}
