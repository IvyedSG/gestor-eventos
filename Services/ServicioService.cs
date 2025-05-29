using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using GestorEventos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public async Task<List<ServicioApi>> GetServiciosAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de servicios");

                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");

                    token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];

                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies");
                        return new List<ServicioApi>();
                    }
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/servicios");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener servicios. Código: {StatusCode}, Mensaje: {Message}",
                        (int)response.StatusCode, response.ReasonPhrase);
                    return new List<ServicioApi>();
                }

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

        public async Task<ServicioApi> CreateServicioAsync(ServicioCreateModel servicio)
        {
            try
            {
                _logger.LogInformation("Creando nuevo servicio: {NombreServicio}", servicio.NombreServicio);

                // Get token from cookies or user claims
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return null;
                    }
                }

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Ensure all precioActual values are strings
                foreach (var item in servicio.Items)
                {
                    // If precioActual is not already a string, convert it
                    if (item.PrecioActual != null && !item.PrecioActual.GetType().Equals(typeof(string)))
                    {
                        item.PrecioActual = item.PrecioActual.ToString();
                    }
                }

                var jsonContent = JsonSerializer.Serialize(servicio);
                _logger.LogDebug("Enviando datos al API: {JsonContent}", jsonContent);

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/servicios", content);

                _logger.LogDebug("Respuesta recibida: Status {StatusCode}", (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear servicio. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}",
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", responseContent);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var nuevoServicio = JsonSerializer.Deserialize<ServicioApi>(responseContent, options);
                _logger.LogInformation("Servicio creado exitosamente con ID: {Id}", nuevoServicio?.Id);

                return nuevoServicio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<ServicioApi> UpdateServicioAsync(string id, ServicioEditModel servicio)
        {
            try
            {
                _logger.LogInformation("Actualizando servicio con ID: {Id}", id);

                // Get token from cookies or user claims
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return null;
                    }
                }

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Ensure all precioActual values are strings
                foreach (var item in servicio.ItemsToAdd)
                {
                    if (item.PrecioActual != null && !item.PrecioActual.GetType().Equals(typeof(string)))
                    {
                        item.PrecioActual = item.PrecioActual.ToString();
                    }
                }

                var jsonContent = JsonSerializer.Serialize(servicio);
                _logger.LogDebug("Enviando datos al API: {JsonContent}", jsonContent);

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/servicios/{id}", content);

                _logger.LogDebug("Respuesta recibida: Status {StatusCode}", (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar servicio. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}",
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", responseContent);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var servicioActualizado = JsonSerializer.Deserialize<ServicioApi>(responseContent, options);
                _logger.LogInformation("Servicio actualizado exitosamente con ID: {Id}", servicioActualizado?.Id);

                return servicioActualizado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<ServicioApi> GetServicioByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo servicio con ID: {Id}", id);

                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return null;
                    }
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/servicios/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener servicio. Código: {StatusCode}, Mensaje: {Message}",
                        (int)response.StatusCode, response.ReasonPhrase);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<ServicioApi>(content, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicio por ID: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> DeleteServicioAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando servicio con ID: {Id}", id);

                // Get token from cookies or user claims
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogWarning("Token no encontrado en las cookies ni en las claims");
                        return false;
                    }
                }

                // Set authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Send DELETE request
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/servicios/{id}");

                _logger.LogDebug("Respuesta recibida: Status {StatusCode}", (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar servicio. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}",
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Servicio eliminado exitosamente: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<List<ServicioApi>> GetAllServiciosAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de servicios para selector");
                return await GetServiciosAsync(); // Reuse your existing method
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los servicios: {Message}", ex.Message);
                return new List<ServicioApi>();
            }
        }
    }
}