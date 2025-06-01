using NRI.Services;
using NRI.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NRI.API
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JwtService _jwtService;
        private readonly string _baseUrl = "http://localhost:5000/api/"; // Укажите ваш базовый URL

        public ApiClient(HttpClient httpClient, JwtService jwtService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                if (Application.Current.Properties.Contains("JwtToken"))
                {
                    var token = Application.Current.Properties["JwtToken"]?.ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"API request failed: {ex.Message}");
                throw;
            }
        }

        // Добавляем новый метод
        public async Task<List<UserStatusDto>> GetOnlineUsersAsync()
        {
            return await GetAsync<List<UserStatusDto>>("api/useractivity/active");
        }
    }
}
