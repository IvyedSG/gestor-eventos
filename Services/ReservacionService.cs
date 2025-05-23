using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json; // Add this for PostAsJsonAsync and PutAsJsonAsync
using System.Linq;

// Add these using directives to resolve missing types
using gestor_eventos.Models.ApiModels; // For ReservacionApi, ReservacionUpdateModel, ReservacionCreateModel
using GestorEventos.Services; // For ApiSettings

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
 
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario: {Email}", correo);
                    
 
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies para el usuario: {Email}", correo);
                        return new List<ReservacionApi>();
                    }
                }

                _logger.LogInformation("Obteniendo reservaciones para el usuario: {Email}", correo);
                
 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

 
                var response = await _httpClient.GetAsync($"api/reservas/{correo}");
                
 
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
        
        public async Task<bool> UpdateReservacionAsync(string userEmail, Guid reservaId, ReservacionUpdateModel updateModel)
        {
            try
            {
                if (updateModel == null)
                {
                    _logger.LogError("El modelo de actualización es nulo");
                    return false;
                }
                
 
                if (updateModel.itemsToAdd == null)
                    updateModel.itemsToAdd = new List<ReservacionUpdateModel.ItemToAdd>();
                    
                if (updateModel.itemsToRemove == null)
                    updateModel.itemsToRemove = new List<string>();
                    
 
                if (!new[] { "PENDIENTE", "CONFIRMADO", "CANCELADO", "FINALIZADO" }.Contains(updateModel.estado?.ToUpper()))
                {
                    updateModel.estado = "PENDIENTE";
                }

 
                var apiUrl = $"{_apiSettings.BaseUrl}/api/reservas/{Uri.EscapeDataString(userEmail)}/{reservaId}";
                
 
                _logger.LogInformation("Actualizando reserva. URL: {Url}, Datos: {@UpdateModel}", apiUrl, updateModel);
                
 
                
 
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
 
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Token no encontrado para la autenticación");
                        return false;
                    }
                }
                
 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
 
                var response = await _httpClient.PutAsJsonAsync(apiUrl, updateModel);
                
 
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de actualización: {StatusCode}, Contenido: {Content}", 
                                      response.StatusCode, responseContent);
                
 
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al actualizar la reservación {Id}. Código: {StatusCode}. Detalle: {Error}",
                                     reservaId, response.StatusCode, responseContent);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reservación {Id}: {Message}", reservaId, ex.Message);
                return false;
            }
        }
        
        public async Task<(bool Success, string Message)> CreateReservacionAsync(string userEmail, ReservacionCreateModel createModel)
        {
            try
            {
                if (createModel == null)
                {
                    _logger.LogError("El modelo de creación es nulo");
                    return (false, "Los datos de la reserva no son válidos");
                }
                
 
                var apiUrl = $"{_apiSettings.BaseUrl}/api/reservas/{Uri.EscapeDataString(userEmail)}";
                
 
                _logger.LogInformation("Creando reserva. URL: {Url}, Datos: {@CreateModel}", apiUrl, createModel);
                
 
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
 
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Token no encontrado para la autenticación");
                        return (false, "No se pudo autenticar al usuario");
                    }
                }
                
 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
 
                var response = await _httpClient.PostAsJsonAsync(apiUrl, createModel);
                
 
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de creación: {StatusCode}, Contenido: {Content}", 
                                      response.StatusCode, responseContent);
                
 
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error al crear la reservación. Código: {StatusCode}. Detalle: {Error}",
                                     response.StatusCode, responseContent);
                    return (false, $"Error del servidor: {response.StatusCode}");
                }
                
                return (true, "Reserva creada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reservación: {Message}", ex.Message);
                return (false, $"Error: {ex.Message}");
            }
        }
        
        public async Task<bool> DeleteReservacionAsync(string correo, Guid reservacionId)
        {
            try
            {
 
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar eliminar reservación: {Id}", reservacionId);
                    
 
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies para el usuario: {Email}", correo);
                        return false;
                    }
                }

                _logger.LogInformation("Eliminando reservación: {Id} para usuario {Email}", reservacionId, correo);
                
 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

 
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/reservas/{correo}/{reservacionId}");
                
 
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar reservación. Código: {StatusCode}, Mensaje: {Message}, Detalles: {Details}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }

                _logger.LogInformation("Reservación eliminada exitosamente: {Id}", reservacionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reservación {Id}: {Message}", reservacionId, ex.Message);
                return false;
            }
        }
        
        public async Task<ReservacionApi> GetReservacionAsync(string id)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar obtener reservación: {Id}", id);
                    return null;
                }

                _logger.LogInformation("Obteniendo reservación: {Id}", id);
                
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/reservaciones/{id}");
                
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener la reservación: {Message}", ex.Message);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta del servidor: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return null;
            }
        }
    }
}