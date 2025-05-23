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
using gestor_eventos.Models;

namespace gestor_eventos.Services
{
    public class InventarioService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<InventarioService> _logger;

        public InventarioService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<InventarioService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<List<InventarioItemApi>> GetInventarioItemsAsync()
        {
            try
            {
                // Obtener token de autenticación
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return new List<InventarioItemApi>();
                }

                _logger.LogInformation("Obteniendo todos los ítems de inventario");
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/items");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener ítems. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return new List<InventarioItemApi>();
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<InventarioItemApi>>(content, options) ?? new List<InventarioItemApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ítems de inventario: {Message}", ex.Message);
                return new List<InventarioItemApi>();
            }
        }

        public async Task<bool> CreateInventarioItemAsync(InventarioItemApi item)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar crear ítem");
                    return false;
                }

                _logger.LogInformation("Creando nuevo ítem de inventario: {Nombre}", item.Nombre);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Crear el objeto exacto que la API espera
                var itemRequest = new
                {
                    nombre = item.Nombre,
                    descripcion = item.Descripcion ?? string.Empty,
                    stock = item.Stock,
                    preciobase = item.PrecioBase ?? "0"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(itemRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/items", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear ítem. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Ítem creado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ítem de inventario: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateInventarioItemAsync(string itemId, InventarioItemApi item)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar actualizar ítem");
                    return false;
                }

                _logger.LogInformation("Actualizando ítem de inventario: {Id}", itemId);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Crear el objeto exacto que la API espera
                var itemRequest = new
                {
                    nombre = item.Nombre,
                    descripcion = item.Descripcion ?? string.Empty,
                    stock = item.Stock,
                    preciobase = item.PrecioBase ?? "0"
                    // Ya no se incluye categoría
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(itemRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/items/{itemId}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar ítem. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Ítem actualizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ítem de inventario: {Message}", ex.Message);
                return false;
            }
        }
    }
}