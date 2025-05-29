using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using gestor_eventos.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GestorEventos.Services;

namespace gestor_eventos.Services
{
    public class InventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<InventoryService> _logger;
        private readonly ApiSettings _apiSettings;

        public InventoryService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<InventoryService> logger,
            ApiSettings apiSettings)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _apiSettings = apiSettings;
        }

        public async Task<List<InventarioItemApi>> GetInventoryItemsAsync()
        {
            try
            {
                _logger.LogInformation("Getting inventory items");
                
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token not found in user claims, trying in cookie");
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("No authentication token found");
                        return new List<InventarioItemApi>();
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/items");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error getting inventory items. Status: {StatusCode}, Message: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return new List<InventarioItemApi>();
                }
                
                var content = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var items = JsonSerializer.Deserialize<List<InventarioItemApi>>(content, options) ?? new List<InventarioItemApi>();
                _logger.LogInformation("Successfully retrieved {Count} inventory items", items.Count);
                
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInventoryItemsAsync: {Message}", ex.Message);
                return new List<InventarioItemApi>();
            }
        }
    }
}