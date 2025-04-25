using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Options;
using GestorEventos.Models.ApiModels;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using gestor_eventos.Models.ApiModels;

namespace GestorEventos.Services
{
    public class ClienteService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor, 
            IOptions<ApiSettings> apiSettings,
            ILogger<ClienteService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<List<ClienteApi>> GetClientesByUsuarioAsync(string correo)
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
                        return new List<ClienteApi>();
                    }
                }

                _logger.LogInformation("Obteniendo clientes para el usuario: {Email}", correo);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición al API
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/clientes/usuario/{correo}");
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error al obtener clientes. Código: {StatusCode}, Mensaje: {Message}", 
                        (int)response.StatusCode, response.ReasonPhrase);
                        
                    return new List<ClienteApi>();
                }

                // Leer y deserializar la respuesta
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta del API: {Response}", content);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<List<ClienteApi>>(content, options) ?? new List<ClienteApi>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión al obtener los clientes: {Message}", ex.Message);
                return new List<ClienteApi>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al procesar la respuesta del servidor: {Message}", ex.Message);
                return new List<ClienteApi>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return new List<ClienteApi>();
            }
        }

        public async Task<bool> CreateClienteAsync(string userEmail, ClienteCreateDto cliente)
        {
            try
            {
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar crear cliente para: {Email}", userEmail);
                    return false;
                }

                _logger.LogInformation("Creando cliente para el usuario: {Email}", userEmail);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Crear el contenido de la petición
                var jsonContent = JsonSerializer.Serialize(cliente);
                _logger.LogDebug("Enviando datos al API: {JsonContent}", jsonContent);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Realizar la petición POST al API
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/clientes/{userEmail}", content);
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al crear cliente. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Cliente creado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateClienteAsync(string clienteId, ClienteCreateDto cliente)
        {
            try
            {
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar actualizar cliente: {Id}", clienteId);
                    return false;
                }

                _logger.LogInformation("Actualizando cliente: {Id}", clienteId);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Crear el contenido de la petición
                var jsonContent = JsonSerializer.Serialize(cliente);
                _logger.LogDebug("Enviando datos al API: {JsonContent}", jsonContent);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Realizar la petición PUT al API
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/clientes/{clienteId}", content);
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al actualizar cliente. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Cliente actualizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteClienteAsync(string clienteId)
        {
            try
            {
                // Obtener el token del usuario actual
                var token = _httpContextAccessor.HttpContext.User.FindFirst("AccessToken")?.Value;
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado al intentar eliminar cliente: {Id}", clienteId);
                    return false;
                }

                _logger.LogInformation("Eliminando cliente: {Id}", clienteId);
                
                // Configurar el header de autenticación
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Realizar la petición DELETE al API
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/clientes/{clienteId}");
                
                // Verificar si la respuesta fue exitosa
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Error al eliminar cliente. Código: {StatusCode}, Mensaje: {Message}, Detalle: {Detail}", 
                        (int)response.StatusCode, response.ReasonPhrase, errorContent);
                    return false;
                }

                _logger.LogInformation("Cliente eliminado exitosamente: {Id}", clienteId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}: {Message}", clienteId, ex.Message);
                return false;
            }
        }
    }
}