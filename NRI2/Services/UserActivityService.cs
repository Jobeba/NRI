// UserActivityService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NRI.Data;
using NRI.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NRI.Services
{
    public class UserActivityService : IUserActivityService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserActivityService> _logger;
        private readonly Func<int> _userIdFactory;
        private readonly AppDbContext _context;
        private Timer _timer;
        private readonly TimeSpan _activityUpdateInterval = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _onlineThreshold = TimeSpan.FromMinutes(5);

        public UserActivityService(
            HttpClient httpClient,
            ILogger<UserActivityService> logger,
            Func<int> userIdFactory,
            IConfiguration configuration,
            AppDbContext context)
        {
            _httpClient = httpClient;
            _logger = logger;
            _userIdFactory = userIdFactory;
            _context = context;
            _httpClient.BaseAddress = new Uri(configuration["ApiBaseUrl"]);
        }

        public void StartTracking()
        {
            _timer = new Timer(async _ => await SendActivityUpdate(),
                null,
                TimeSpan.Zero,
                _activityUpdateInterval);
        }

        public void StopTracking()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task SendActivityUpdate()
        {
            try
            {
                var userId = _userIdFactory();
                if (userId <= 0) return;

                var response = await _httpClient.PostAsJsonAsync(
                    "api/useractivity/update",
                    new { UserId = userId });

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user activity");
            }
        }

        public async Task<List<UserStatusDto>> GetActiveUsersAsync()
        {
            try
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
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка активных пользователей");
                return new List<UserStatusDto>();
            }
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var now = DateTime.UtcNow;

            return new UserStatisticsDto
            {
                TotalUsers = users.Count,
                OnlineUsers = users.Count(u => now - u.LastActivity < _onlineThreshold),
                OfflineUsers = users.Count(u => now - u.LastActivity >= _onlineThreshold),
                LastUpdated = DateTime.Now
            };
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
