using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Threading;

namespace NRI.Services
{
    public class UserActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly int _currentUserId; // Храним ID пользователя
        private readonly DispatcherTimer _activityTimer;

        public UserActivityService(HttpClient httpClient, int currentUserId)
        {
            _httpClient = httpClient;
            _currentUserId = currentUserId;
            _activityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _activityTimer.Tick += OnActivityTick;
        }

        public void StartTracking()
        {
            _activityTimer.Start();
        }

        private async void OnActivityTick(object sender, EventArgs e)
        {
            try
            {
                await _httpClient.PostAsJsonAsync("api/useractivity/update", _currentUserId);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка отправки активности: {ex.Message}");
            }
        }
    }
}
