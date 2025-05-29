using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Linq;
using gestor_eventos.Models.ApiModels;
using GestorEventos.Services;

namespace gestor_eventos.Services
{
    public class ReservacionService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReservacionService> _logger;

        public ReservacionService(
            HttpClient httpClient, 
            IOptions<ApiSettings> apiSettings,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReservacionService> logger)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
        }

        public async Task<List<ReservacionApi>> GetAllReservacionesAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las reservaciones");

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return new List<ReservacionApi>();
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/reservas");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener reservaciones. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<ReservacionApi>();
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<ReservacionApi>>(content, options) ?? new List<ReservacionApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener todas las reservaciones: {Message}", ex.Message);
                return new List<ReservacionApi>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta del servidor: {Message}", ex.Message);
                return new List<ReservacionApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return new List<ReservacionApi>();
            }
        }

        public async Task<ReservacionApi> CreateReservacionAsync(ReservacionCreateModel reservacion)
        {
            try
            {
                _logger.LogInformation("Creando nueva reservación: {NombreEvento}", reservacion.NombreEvento);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return null;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Log all data being sent to API
                var jsonContent = JsonSerializer.Serialize(reservacion);
                _logger.LogDebug("Enviando datos al API: {JsonContent}", jsonContent);
                
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                // Log the full URL being called
                var url = $"{_apiSettings.BaseUrl}/api/reservas";
                _logger.LogInformation("Calling API URL: {Url}", url);
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear reservación. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return null;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", responseContent);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var nuevaReservacion = JsonSerializer.Deserialize<ReservacionApi>(responseContent, options);
                _logger.LogInformation("Reservación creada exitosamente con ID: {Id}", nuevaReservacion?.Id);
                
                return nuevaReservacion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reservación: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<ReservacionApi> GetReservacionByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo reservación con ID: {Id}", id);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return null;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/reservas/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener reservación. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<ReservacionApi>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservación por ID: {Message}", ex.Message);
                return null;
            }
        }
        
        public async Task<ReservacionApi> UpdateReservacionAsync(string id, object rawData)
        {
            try
            {
                _logger.LogInformation("Actualizando reservación con ID: {Id}", id);

                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado");
                        return null;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Convertimos directamente a JSON sin modificar la estructura
                var jsonContent = JsonSerializer.Serialize(rawData);
                _logger.LogInformation("Payload enviado a la API: {JsonContent}", jsonContent);
                
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var url = $"{_apiSettings.BaseUrl}/api/reservas/{id}";
                _logger.LogInformation("URL de la API: {Url}", url);
                
                var response = await _httpClient.PutAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de la API: Status {Status}, Contenido: {Content}", 
                    (int)response.StatusCode, responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al actualizar reserva. Status: {Status}, Detalle: {Detail}", 
                        (int)response.StatusCode, responseContent);
                    return null;
                }
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ReservacionApi>(responseContent, options);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reservación: {Message}", ex.Message);
                return null;
            }
        }
    }
}