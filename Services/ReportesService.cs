using GestorEventos.Models.ApiModels;
using System.Text.Json;
using GestorEventos.Models; // Add this if ApiSettings is in GestorEventos.Models namespace

namespace gestor_eventos.Services
{
    public class ReportesService
    {
        private readonly HttpClient _httpClient;
        private readonly GestorEventos.Services.ApiSettings _apiSettings;
        private readonly ILogger<ReportesService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportesService(HttpClient httpClient, GestorEventos.Services.ApiSettings apiSettings, ILogger<ReportesService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _apiSettings = apiSettings;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetAuthToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("AuthToken") ?? string.Empty;
        }

        private void SetAuthHeader()
        {
            var token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }
        }

        public async Task<ResumenEjecutivoResponse?> GetResumenEjecutivoAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/resumen-ejecutivo{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ResumenEjecutivoResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Error al obtener resumen ejecutivo. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen ejecutivo");
                return null;
            }
        }

        public async Task<ReportesClientesResponse?> GetReportesClientesAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/clientes{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReportesClientesResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Error al obtener reportes de clientes. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de clientes");
                return null;
            }
        }

        public async Task<ReportesItemsResponse?> GetReportesItemsAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                query += string.IsNullOrEmpty(query) ? "?" : "&";
                query += $"top={top}";
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/items{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReportesItemsResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Error al obtener reportes de items. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de items");
                return null;
            }
        }

        public async Task<ReportesPagosResponse?> GetReportesPagosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/pagos{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content) || content.Trim() == "null")
                    {
                        _logger.LogWarning("Contenido de pagos es null o vacío");
                        return null;
                    }
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    return JsonSerializer.Deserialize<ReportesPagosResponse>(content, options);
                }

                _logger.LogWarning("Error al obtener reportes de pagos. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error al deserializar reportes de pagos");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de pagos");
                return null;
            }
        }

        private ReportesPagosResponse ApplyDateFilterToPagosResponse(ReportesPagosResponse response, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (response == null) return response;

            try
            {
                // Para este caso específico, como es un filtrado simple en el cliente,
                // podemos filtrar la tendencia mensual si tenemos esos datos
                if (response.TendenciaMensualIngresos != null && (fechaInicio.HasValue || fechaFin.HasValue))
                {
                    var filteredTendencia = response.TendenciaMensualIngresos.Where(t =>
                    {
                        // Crear una fecha aproximada del mes para comparar
                        var fechaMes = new DateTime(t.Año, t.Mes, 1);
                        
                        if (fechaInicio.HasValue && fechaMes < fechaInicio.Value.Date)
                            return false;
                            
                        if (fechaFin.HasValue && fechaMes > fechaFin.Value.Date)
                            return false;
                            
                        return true;
                    }).ToList();

                    response.TendenciaMensualIngresos = filteredTendencia;
                }

                _logger.LogInformation("Filtrado manual aplicado. Registros de tendencia: {Count}", 
                    response.TendenciaMensualIngresos?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al aplicar filtrado manual, retornando datos sin filtrar");
            }

            return response;
        }

        public async Task<ReportesReservasResponse?> GetReportesReservasAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/reservas{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReportesReservasResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Error al obtener reportes de reservas. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de reservas");
                return null;
            }
        }

        public async Task<ReportesServiciosResponse?> GetReportesServiciosAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 10)
        {
            try
            {
                SetAuthHeader();
                var query = BuildDateQuery(fechaInicio, fechaFin);
                query += string.IsNullOrEmpty(query) ? "?" : "&";
                query += $"top={top}";
                
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}/api/Reportes/servicios{query}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReportesServiciosResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogWarning("Error al obtener reportes de servicios. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de servicios");
                return null;
            }
        }

        private string BuildDateQuery(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var queryParams = new List<string>();

            if (fechaInicio.HasValue)
            {
                queryParams.Add($"fechaInicio={fechaInicio.Value:yyyy-MM-ddTHH:mm:ss}");
            }

            if (fechaFin.HasValue)
            {
                queryParams.Add($"fechaFin={fechaFin.Value:yyyy-MM-ddTHH:mm:ss}");
            }

            return queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
        }
    }
}