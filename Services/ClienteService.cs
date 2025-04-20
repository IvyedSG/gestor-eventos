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
                    
                    // Devolvemos una lista vacía en lugar de lanzar excepción
                    // para manejar el caso de forma más elegante en la UI
                    return new List<ClienteApi>();
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
    }
}