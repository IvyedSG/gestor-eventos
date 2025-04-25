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
using gestor_eventos.Models.ApiModels;

namespace gestor_eventos.Services
{
    public class ServicioService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<ServicioService> _logger;

        public ServicioService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<ServicioService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<List<ServicioApi>> GetServiciosByCorreoAsync(string correo)
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
                        return new List<ServicioApi>();
                    }
                }

                _logger.LogInformation("Obteniendo servicios para el usuario: {Email}", correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición al API
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/servicios/{correo}");
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener servicios. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<ServicioApi>();
                }

                // Leer y deserializar la respuesta
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<ServicioApi>>(content, options) ?? new List<ServicioApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener los servicios: {Message}", ex.Message);
                return new List<ServicioApi>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta del servidor: {Message}", ex.Message);
                return new List<ServicioApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return new List<ServicioApi>();
            }
        }

        public async Task<bool> CreateServicioAsync(string correo, ServicioCreateModel servicioModel)
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
                        return false;
                    }
                }

                _logger.LogInformation("Creando nuevo servicio para el usuario: {Email}", correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Serializar el modelo a JSON
                var content = new StringContent(
                    JsonSerializer.Serialize(servicioModel), 
                    System.Text.Encoding.UTF8, 
                    "application/json");

                // Realizar la petición al API
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/servicios/{correo}", content);
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear servicio. Código: {StatusCode}, Mensaje: {Message}, Detalles: {Details}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }

                _logger.LogInformation("Servicio creado exitosamente para el usuario: {Email}", correo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateServicioAsync(string correo, ServicioUpdateModel servicioModel)
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
                        return false;
                    }
                }

                _logger.LogInformation("Actualizando servicio para el usuario: {Email}", correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Serializar el modelo a JSON
                var content = new StringContent(
                    JsonSerializer.Serialize(servicioModel), 
                    System.Text.Encoding.UTF8, 
                    "application/json");

                // Realizar la petición al API
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/servicios/{correo}", content);
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar servicio. Código: {StatusCode}, Mensaje: {Message}, Detalles: {Details}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }

                _logger.LogInformation("Servicio actualizado exitosamente para el usuario: {Email}", correo);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateServicioDetailedAsync(string correo, string servicioId, ServicioUpdateRequestModel model)
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
                        return false;
                    }
                }

                _logger.LogInformation("Actualizando servicio con ID {Id} para el usuario: {Email}", servicioId, correo);
                _logger.LogInformation("Datos de actualización: {Data}", JsonSerializer.Serialize(model));
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Serializar el modelo a JSON
                var content = new StringContent(
                    JsonSerializer.Serialize(model), 
                    System.Text.Encoding.UTF8, 
                    "application/json");

                // Realizar la petición al API con el nuevo endpoint
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/servicios/{correo}/{servicioId}", content);
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar servicio detallado. Código: {StatusCode}, Mensaje: {Message}, Detalles: {Details}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }

                _logger.LogInformation("Servicio actualizado detalladamente con éxito para el usuario: {Email}, ID: {Id}", correo, servicioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio detallado: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteServicioAsync(string correo, string servicioId)
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
                        return false;
                    }
                }

                _logger.LogInformation("Eliminando servicio con ID {Id} para el usuario: {Email}", servicioId, correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición al API
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/servicios/{correo}/{servicioId}");
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar servicio. Código: {StatusCode}, Mensaje: {Message}, Detalles: {Details}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                        
                    return false;
                }

                _logger.LogInformation("Servicio eliminado exitosamente para el usuario: {Email}, ID: {Id}", correo, servicioId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio: {Message}", ex.Message);
                return false;
            }
        }
    }
}