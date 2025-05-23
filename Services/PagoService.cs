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
using gestor_eventos.Models.ApiModels;

namespace gestor_eventos.Services
{
    public class PagoService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<PagoService> _logger;

        public PagoService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings,
            ILogger<PagoService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async Task<List<PagoDto>> GetPagosByReservaAsync(string reservaId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return new List<PagoDto>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/pagos/reserva/{reservaId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<PagoDto>>(content, options) ?? new List<PagoDto>();
                }
                
                _logger.LogError($"Error al obtener los pagos: {response.StatusCode}");
                return new List<PagoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los pagos");
                return new List<PagoDto>();
            }
        }

        public async Task<List<PagoDto>> GetPagosByUsuarioAsync(string email)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return new List<PagoDto>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/pagos/usuario/{email}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<PagoDto>>(content, options) ?? new List<PagoDto>();
                }
                
                _logger.LogError($"Error al obtener los pagos del usuario: {response.StatusCode}");
                return new List<PagoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los pagos del usuario");
                return new List<PagoDto>();
            }
        }

        public async Task<bool> CreatePagoAsync(PagoDto pago)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var content = new StringContent(
                    JsonSerializer.Serialize(pago),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BaseUrl}/api/pagos", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogError($"Error al crear el pago: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el pago");
                return false;
            }
        }

        public async Task<bool> UpdatePagoAsync(string id, PagoDto pago)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var content = new StringContent(
                    JsonSerializer.Serialize(pago),
                    System.Text.Encoding.UTF8,
                    "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiSettings.BaseUrl}/api/pagos/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogError($"Error al actualizar el pago: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el pago");
                return false;
            }
        }

        public async Task<bool> DeletePagoAsync(string id)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.User?.FindFirst("AccessToken")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token no encontrado en las claims del usuario");
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.DeleteAsync($"{_apiSettings.BaseUrl}/api/pagos/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogError($"Error al eliminar el pago: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el pago");
                return false;
            }
        }
    }
}