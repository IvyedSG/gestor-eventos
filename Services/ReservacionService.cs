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
        
        public async Task<bool> UpdateReservacionAsync(string userEmail, Guid reservaId, ReservacionUpdateModel updateModel)
        {
            try
            {
                if (updateModel == null)
                {
                    _logger.LogError("El modelo de actualización es nulo");
                    return false;
                }
                
                // Asegurarse de que las listas nunca sean nulas
                if (updateModel.itemsToAdd == null)
                    updateModel.itemsToAdd = new List<ReservacionUpdateModel.ItemToAdd>();
                    
                if (updateModel.itemsToRemove == null)
                    updateModel.itemsToRemove = new List<string>();
                    
                // Validar el estado
                if (!new[] { "PENDIENTE", "CONFIRMADO", "CANCELADO", "FINALIZADO" }.Contains(updateModel.estado?.ToUpper()))
                {
                    updateModel.estado = "PENDIENTE";
                }

                // Preparar la URL correcta - Usa _apiSettings.BaseUrl en lugar de _baseUrl
                var apiUrl = $"{_apiSettings.BaseUrl}/api/reservas/{Uri.EscapeDataString(userEmail)}/{reservaId}";
                
                // Registrar la solicitud que se va a enviar
                _logger.LogInformation("Actualizando reserva. URL: {Url}, Datos: {@UpdateModel}", apiUrl, updateModel);
                
                // Usar el _httpClient que ya está inyectado en lugar de crear uno nuevo
                
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    // Intentar obtener el token de las cookies
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Token no encontrado para la autenticación");
                        return false;
                    }
                }
                
                // Configurar la autorización para esta solicitud específica
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Enviar la solicitud PUT con el modelo de actualización
                var response = await _httpClient.PutAsJsonAsync(apiUrl, updateModel);
                
                // Registrar la respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de actualización: {StatusCode}, Contenido: {Content}", 
                                      response.StatusCode, responseContent);
                
                // Verificar si la solicitud fue exitosa
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
                
                // Preparar la URL correcta
                var apiUrl = $"{_apiSettings.BaseUrl}/api/reservas/{Uri.EscapeDataString(userEmail)}";
                
                // Registrar la solicitud que se va a enviar
                _logger.LogInformation("Creando reserva. URL: {Url}, Datos: {@CreateModel}", apiUrl, createModel);
                
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    // Intentar obtener el token de las cookies
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Token no encontrado para la autenticación");
                        return (false, "No se pudo autenticar al usuario");
                    }
                }
                
                // Configurar la autorización para esta solicitud específica
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                // Enviar la solicitud POST con el modelo de creación
                var response = await _httpClient.PostAsJsonAsync(apiUrl, createModel);
                
                // Registrar la respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Respuesta de creación: {StatusCode}, Contenido: {Content}", 
                                      response.StatusCode, responseContent);
                
                // Verificar si la solicitud fue exitosa
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
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar eliminar reservación: {Id}", reservacionId);
                    
                    // Intentar obtener el token de las cookies
                    token = _httpContextAccessor.HttpContext.Request.Cookies["AuthToken"];
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies para el usuario: {Email}", correo);
                        return false;
                    }
                }

                _logger.LogInformation("Eliminando reservación: {Id} para usuario {Email}", reservacionId, correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición DELETE al API
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/reservas/{correo}/{reservacionId}");
                
                // Verificar si la respuesta fue exitosa
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
        
        // Agregar aquí métodos adicionales para crear, actualizar o eliminar reservaciones
    }
}