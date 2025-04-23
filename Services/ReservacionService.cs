using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using Microsoft.Extensions.Options;
using GestorEventos.Services;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

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

        public async Task<IEnumerable<ReservacionApi>> GetReservacionesByCorreoAsync(string correo)
        {
            try
            {
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario: {Email}", correo);
                    
                    // Intentar obtener el token de las cookies
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies para el usuario: {Email}", correo);
                        return new List<ReservacionApi>();
                    }
                }

                _logger.LogInformation("Obteniendo reservaciones para el usuario: {Email}", correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición al API
                var response = await _httpClient.GetAsync($"api/reservas/{correo}");
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener reservaciones. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<ReservacionApi>();
                }

                // Leer y deserializar la respuesta
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<IEnumerable<ReservacionApi>>(content, options) ?? new List<ReservacionApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener reservaciones: {Message}", ex.Message);
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
        
        // Agregar aquí métodos adicionales para crear, actualizar o eliminar reservaciones
    }
}