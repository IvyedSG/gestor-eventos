using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using GestorEventos.Services;

namespace gestor_eventos.Services
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<InventoryService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<List<InventoryItem>> GetInventoryItemsByUserIdAsync(string userId)
        {
            try
            {
                // Obtain the token from the current user
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token not found in user claims for user ID: {UserId}", userId);
                    return new List<InventoryItem>();
                }

                _logger.LogInformation("Getting inventory items for user ID: {UserId}", userId);
                
                // Set authentication header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Make the API request
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/inventario/usuario/{userId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error getting inventory items. Status: {StatusCode}, Message: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return new List<InventoryItem>();
                }

                // Read and deserialize the response
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API Response: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<InventoryItem>>(content, options) ?? new List<InventoryItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory items: {Message}", ex.Message);
                return new List<InventoryItem>();
            }
        }
    }

    public class InventoryItem
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
    }
}