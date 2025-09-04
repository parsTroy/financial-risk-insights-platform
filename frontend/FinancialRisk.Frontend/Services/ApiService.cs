using System.Net.Http.Json;
using System.Text.Json;
using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation("Making GET request to: {Endpoint}", endpoint);
                var response = await _httpClient.GetFromJsonAsync<T>(endpoint, _jsonOptions);
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for endpoint: {Endpoint}", endpoint);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout for endpoint: {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                _logger.LogInformation("Making POST request to: {Endpoint}", endpoint);
                _logger.LogInformation("HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response: {Response}", responseContent);
                
                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP POST request failed for endpoint: {Endpoint}", endpoint);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "POST request timeout for endpoint: {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<bool> PostAsync(string endpoint, object data)
        {
            try
            {
                _logger.LogInformation("Making POST request to: {Endpoint}", endpoint);
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP POST request failed for endpoint: {Endpoint}", endpoint);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "POST request timeout for endpoint: {Endpoint}", endpoint);
                return false;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                _logger.LogInformation("Making PUT request to: {Endpoint}", endpoint);
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP PUT request failed for endpoint: {Endpoint}", endpoint);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "PUT request timeout for endpoint: {Endpoint}", endpoint);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("Making DELETE request to: {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP DELETE request failed for endpoint: {Endpoint}", endpoint);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "DELETE request timeout for endpoint: {Endpoint}", endpoint);
                return false;
            }
        }
    }
}
