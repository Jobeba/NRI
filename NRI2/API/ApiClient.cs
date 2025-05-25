using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; 
using System.Threading.Tasks;
using System.Windows; // Для Application.Current
using NRI.Services;

namespace NRI.API
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JwtService _jwtService;

        public ApiClient(HttpClient httpClient, JwtService jwtService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                if (Application.Current.Properties.Contains("JwtToken"))
                {
                    var token = Application.Current.Properties["JwtToken"]?.ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API request failed: {ex.Message}");
                throw;
            }
        }
    }
}
