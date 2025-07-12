using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using GestorEventos.Models.ApiModels;
using GestorEventos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gestor_eventos.Services
{
    public class DashboardService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<DashboardService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<DashboardResponse> GetDashboardDataAsync(string correo)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario: {Email}", correo);
                    
 
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies para el usuario: {Email}", correo);
                        return null;
                    }
                }

                _logger.LogInformation("Obteniendo datos del dashboard para el usuario: {Email}", correo);
                
 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

 
                // Verificar que el endpoint coincida con tu API
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/dashboard/{correo}");
                
 
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener datos del dashboard. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return null;
                }

 
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<DashboardResponse>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del dashboard para {Email}: {Message}", correo, ex.Message);
                return null;
            }
        }
    }
}