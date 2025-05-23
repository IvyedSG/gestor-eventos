using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using gestor_eventos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace gestor_eventos.Pages.Inventario
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        
        public List<InventarioItemApi> InventoryItems { get; set; }
        public bool HasLowStockItems { get; set; }
        
        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;
        }
        
        public async Task OnGetAsync()
        {
 
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            
 
            await FetchInventoryItemsFromApi();
            
 
            ApplyFilters();
            
 
            TotalItems = InventoryItems.Count;
            
 
            InventoryItems = InventoryItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
 
            HasLowStockItems = InventoryItems.Any(i => i.Stock < 10);
        }
        
        private async Task FetchInventoryItemsFromApi()
        {
            try
            {
                // Ya no necesitamos obtener el ID de usuario
                var client = _clientFactory.CreateClient();
                
                // Obtenemos el token de autenticación como antes
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                string baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://api.example.com";
                
                // Utilizamos el nuevo endpoint que no requiere el ID de usuario
                _logger.LogInformation($"Llamando a la API: {baseUrl}/api/items");
                
                var response = await client.GetAsync($"{baseUrl}/api/items");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Respuesta API: {content}");
                    
                    InventoryItems = JsonSerializer.Deserialize<List<InventarioItemApi>>(content, options);
                    
                    _logger.LogInformation($"Successfully retrieved {InventoryItems.Count} inventory items from API");
                }
                else
                {
                    _logger.LogError($"Failed to retrieve inventory items. API returned {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error details: {errorContent}");
                    InventoryItems = new List<InventarioItemApi>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory items from API");
                InventoryItems = new List<InventarioItemApi>();
            }
        }
        
        private void ApplyFilters()
        {
 
            if (InventoryItems == null)
                return;
                
 
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                InventoryItems = InventoryItems.Where(i => 
                    i.Nombre.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                    i.Descripcion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            
 
            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                if (StatusFilter == "Normal")
                {
                    InventoryItems = InventoryItems.Where(i => i.Stock >= 10).ToList();
                }
                else if (StatusFilter == "Bajo")
                {
                    InventoryItems = InventoryItems.Where(i => i.Stock > 0 && i.Stock < 10).ToList();
                }
                else if (StatusFilter == "Agotado")
                {
                    InventoryItems = InventoryItems.Where(i => i.Stock <= 0).ToList();
                }
            }
        }

 
        public async Task<IActionResult> OnPostSaveItemAsync([FromBody] InventarioItemApi newItem)
        {
            try
            {
                _logger.LogInformation($"Recibidos datos para nuevo ítem: {JsonSerializer.Serialize(newItem)}");
                
                if (newItem == null)
                {
                    _logger.LogWarning("Datos de ítem recibidos como nulos");
                    return new JsonResult(new { success = false, message = "Datos de ítem inválidos" });
                }

                if (string.IsNullOrWhiteSpace(newItem.Nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre del ítem es obligatorio" });
                }

                var client = _clientFactory.CreateClient();
                
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                _logger.LogInformation($"Enviando solicitud POST a: {baseUrl}/api/items");

                // Asegúrate de que estos nombres coincidan exactamente con lo que la API espera
                var itemRequest = new
                {
                    nombre = newItem.Nombre,
                    descripcion = newItem.Descripcion ?? string.Empty,
                    stock = newItem.Stock,
                    preciobase = newItem.PrecioBase ?? "0"
                };

                _logger.LogInformation($"Datos a enviar: {JsonSerializer.Serialize(itemRequest)}");

                var content = new StringContent(
                    JsonSerializer.Serialize(itemRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");
            
                var response = await client.PostAsync($"{baseUrl}/api/items", content);
        
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Respuesta API: {response.StatusCode} - {responseContent}");
        
                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = true, message = "Ítem creado correctamente" });
                }
                else
                {
                    _logger.LogWarning($"Error en API: {response.StatusCode} - {responseContent}");
                    return new JsonResult(new { 
                        success = false, 
                        message = $"Error al crear el ítem. Código: {response.StatusCode}", 
                        details = responseContent 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud para crear un nuevo ítem");
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        public async Task<IActionResult> OnPostUpdateItemAsync(string id, [FromBody] InventarioItemApi updatedItem)
        {
            try
            {
                if (updatedItem == null || string.IsNullOrEmpty(id))
                {
                    return new JsonResult(new { success = false, message = "Datos de ítem inválidos o ID no proporcionado" });
                }

                if (string.IsNullOrWhiteSpace(updatedItem.Nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre del ítem es obligatorio" });
                }

                if (updatedItem.Stock < 0)
                {
                    return new JsonResult(new { success = false, message = "El stock no puede ser negativo" });
                }

                var client = _clientFactory.CreateClient();
                
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                _logger.LogInformation($"Enviando solicitud PUT a: {baseUrl}/api/items/{id}");
                _logger.LogInformation($"Datos de actualización: {JsonSerializer.Serialize(updatedItem)}");

                // Crear el objeto con el formato exacto que la API espera
                var itemRequest = new
                {
                    nombre = updatedItem.Nombre,
                    descripcion = updatedItem.Descripcion ?? string.Empty,
                    stock = updatedItem.Stock,
                    preciobase = updatedItem.PrecioBase ?? "0"
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(itemRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");
            
                var response = await client.PutAsync($"{baseUrl}/api/items/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Ítem actualizado correctamente");
                    return new JsonResult(new { success = true, message = "Ítem actualizado correctamente" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al actualizar el ítem. API devolvió {response.StatusCode}: {errorContent}");
                    return new JsonResult(new { 
                        success = false, 
                        message = $"Error al actualizar el ítem. Código: {response.StatusCode}", 
                        details = errorContent 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud para actualizar el ítem");
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        public async Task<IActionResult> OnPostUpdateStockAsync(string id, int newStock)
        {
 
            _logger.LogInformation($"Updating stock for item {id} to {newStock}");
            
 
            await Task.Delay(100);
            
            return new JsonResult(new { success = true, message = "Stock actualizado correctamente" });
        }
        
        public async Task<IActionResult> OnPostDeleteItemAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return new JsonResult(new { success = false, message = "ID de ítem no proporcionado" });
                }

 
                string userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("No se pudo obtener el correo de usuario de las claims");
                    return new JsonResult(new { success = false, message = "No se pudo identificar al usuario" });
                }

 
                var client = _clientFactory.CreateClient();
                
 
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
 
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
 
                _logger.LogInformation($"Enviando solicitud DELETE a: {baseUrl}/api/inventario/{userEmail}/{id}");

 
                var response = await client.DeleteAsync($"{baseUrl}/api/inventario/{userEmail}/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Ítem eliminado correctamente");
                    return new JsonResult(new { success = true, message = "Ítem eliminado correctamente" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al eliminar el ítem. API devolvió {response.StatusCode}: {errorContent}");
                    return new JsonResult(new { 
                        success = false, 
                        message = $"Error al eliminar el ítem. Código: {response.StatusCode}", 
                        details = errorContent 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud para eliminar el ítem");
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        public async Task<IActionResult> OnGetItemDetailsAsync(string id)
        {
            try
            {
 
                var client = _clientFactory.CreateClient();
                
 
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
 
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
 
                var response = await client.GetAsync($"{baseUrl}/api/inventario/item/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var item = JsonSerializer.Deserialize<InventarioItemApi>(content, options);
                    
                    return new JsonResult(item);
                }
                
                _logger.LogError($"Failed to retrieve item details. API returned {response.StatusCode}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching details for item {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}