using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NRI.Classes;
using NRI.Data;
using NRI.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace NRI.Service
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _onlineThreshold = TimeSpan.FromMinutes(5);
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
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

        public async Task<List<User>> GetOnlineUsersAsync()
        {
            var threshold = DateTime.UtcNow.AddMinutes(-5);
            return await _context.Users
                .Where(u => u.LastActivity >= threshold)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<User>> GetAllUserListAsync()
        {
            // Используем ToListAsync() для асинхронного выполнения
            return await _context.Users.ToListAsync();
        }


        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Логирование ошибки
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Логирование ошибки
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return false;

                _context.Users.Remove(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Логирование ошибки
                return false;
            }
        }
    }
}
