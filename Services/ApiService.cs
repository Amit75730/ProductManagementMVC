using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProductManagementMVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            var baseUrl = _configuration["ConnectionStrings:ApiBaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
        }

        /// <summary>
        /// Retrieves the token from the session and adds it to the Authorization header.
        /// </summary>
        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("🚨 Token is missing! User is likely not authenticated.");
            }
            else
            {
                Console.WriteLine($"🔑 Token Found: {token}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Sends a POST request with authorization.
        /// </summary>
        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
            };

            AddAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Unauthorized request. Please log in again.");
            }

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            try
            {
                Console.WriteLine($"🔍 API Response: {responseContent}"); // Debugging Log

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
                };

                return JsonSerializer.Deserialize<T>(responseContent, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ JSON Deserialization Error: {ex.Message}");
                throw new Exception($"JSON Deserialization Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a GET request with authorization.
        /// </summary>
        public async Task<T> GetAsync<T>(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthorizationHeader(request);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("🚨 Unauthorized request. Token may be invalid or expired.");
                throw new UnauthorizedAccessException("Unauthorized request. Please log in again.");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
                };

                return JsonSerializer.Deserialize<T>(responseContent, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ JSON Deserialization Error: {ex.Message}");
                throw new Exception($"JSON Deserialization Error: {ex.Message}");
            }
        }
    }
}
