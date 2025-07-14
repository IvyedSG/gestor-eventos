using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using GestorEventos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gestor_eventos.Services
{
    public class PagoService
    {
        private readonly HttpClient _httpClient;
        private readonly GestorEventos.Services.ApiSettings _apiSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PagoService> _logger;

        public PagoService(
            HttpClient httpClient, 
            IOptions<ApiSettings> apiSettings,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PagoService> logger)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<PagoApi>> GetAllPagosAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los pagos");

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return new List<PagoApi>();
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/pagos");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener pagos. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<PagoApi>();
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<PagoApi>>(content, options) ?? new List<PagoApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener todos los pagos: {Message}", ex.Message);
                return new List<PagoApi>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta del servidor: {Message}", ex.Message);
                return new List<PagoApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return new List<PagoApi>();
            }
        }

        // Nuevo método para obtener todas las reservaciones
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las reservaciones: {Message}", ex.Message);
                return new List<ReservacionApi>();
            }
        }

        // Nuevo método para crear un pago
        public async Task<PagoApi> CreatePagoAsync(PagoCreateModel pago)
        {
            try
            {
                _logger.LogInformation("Creando nuevo pago: {@Pago}", pago);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return null;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var json = JsonSerializer.Serialize(pago);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/pagos", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al crear pago. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                    return null;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<PagoApi>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago: {Message}", ex.Message);
                return null;
            }
        }

        // Nuevo método para actualizar un pago
        public async Task<PagoApi> UpdatePagoAsync(string id, PagoUpdateModel pago)
        {
            try
            {
                _logger.LogInformation("Actualizando pago {Id}: {@Pago}", id, pago);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return null;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var content = new StringContent(
                    JsonSerializer.Serialize(pago),
                    Encoding.UTF8,
                    "application/json"
                );
                
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/pagos/{id}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar pago. Código: {StatusCode}, Mensaje: {Message}, Contenido: {ErrorContent}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return null;
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", responseContent);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<PagoApi>(responseContent, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pago: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> DeletePagoAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando pago {Id}", id);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return false;
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/pagos/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar pago. Código: {StatusCode}, Mensaje: {Message}, Contenido: {ErrorContent}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago: {Message}", ex.Message);
                return false;
            }
        }

        // Nuevo método para obtener pagos por ID de reserva
        public async Task<List<PagoApi>> GetPagosByReservaIdAsync(string reservaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo pagos para la reserva {ReservaId}", reservaId);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return new List<PagoApi>();
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/pagos/reserva/{reservaId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("No se encontraron pagos para la reserva {ReservaId}", reservaId);
                    return new List<PagoApi>();
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al obtener pagos de la reserva. Código: {StatusCode}, Mensaje: {Message}, Contenido: {ErrorContent}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return new List<PagoApi>();
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<PagoApi>>(content, options) ?? new List<PagoApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos de la reserva: {Message}", ex.Message);
                return new List<PagoApi>();
            }
        }

        public async Task<List<PagoApi>> GetPagosForReservacionAsync(string reservaId)
        {
            try
            {
                _logger.LogInformation("Obteniendo pagos para la reservación con ID: {Id}", reservaId);

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return new List<PagoApi>();
                    }
                }
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/pagos/reserva/{reservaId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogInformation("No se encontraron pagos para la reservación: {Id}", reservaId);
                    return new List<PagoApi>();
                }
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener pagos para la reservación. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<PagoApi>();
                }
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<PagoApi>>(content, options) ?? new List<PagoApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos para la reservación: {Message}", ex.Message);
                return new List<PagoApi>();
            }
        }
    }
}