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
        public string CategoryFilter { get; set; }
        
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
            // Ensure we're at least on page 1
            if (CurrentPage < 1)
            {
                CurrentPage = 1;
            }
            
            // Call the API to get inventory items
            await FetchInventoryItemsFromApi();
            
            // Apply filters locally (ideally this should be done on the API side)
            ApplyFilters();
            
            // Count total items for pagination after filtering
            TotalItems = InventoryItems.Count;
            
            // Apply pagination
            InventoryItems = InventoryItems
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            // Check if there are items with low stock (for example, less than 10)
            HasLowStockItems = InventoryItems.Any(i => i.Stock < 10);
        }
        
        private async Task FetchInventoryItemsFromApi()
        {
            try
            {
                // Obtener el userId desde las claims de la sesión del usuario
                string userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("No se pudo obtener el ID de usuario de las claims");
                    InventoryItems = new List<InventarioItemApi>();
                    return;
                }

                // Crear HTTP client
                var client = _clientFactory.CreateClient();
                
                // Obtener el token de autenticación
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                
                // Get the base URL from configuration
                string baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://api.example.com";
                
                // Log de la llamada a la API
                _logger.LogInformation($"Llamando a la API: {baseUrl}/api/inventario/usuario/{userId}");
                
                // Make the API call
                var response = await client.GetAsync($"{baseUrl}/api/inventario/usuario/{userId}");
                
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
            // Apply filters if InventoryItems is not null
            if (InventoryItems == null)
                return;
                
            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                InventoryItems = InventoryItems.Where(i => 
                    i.Nombre.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                    i.Descripcion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();
            }
            
            // Apply category filter
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                InventoryItems = InventoryItems.Where(i => i.Categoria == CategoryFilter).ToList();
            }
            
            // Apply status filter
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

        // Actions
        public async Task<IActionResult> OnPostSaveItemAsync([FromBody] InventarioItemApi newItem)
        {
            try
            {
                if (newItem == null)
                {
                    return new JsonResult(new { success = false, message = "Datos de ítem inválidos" });
                }

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(newItem.Nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre del ítem es obligatorio" });
                }

                if (string.IsNullOrWhiteSpace(newItem.Categoria))
                {
                    return new JsonResult(new { success = false, message = "La categoría es obligatoria" });
                }

                if (newItem.Stock < 0)
                {
                    return new JsonResult(new { success = false, message = "El stock no puede ser negativo" });
                }

                // Obtener el correo del usuario desde las claims
                string userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("No se pudo obtener el correo de usuario de las claims");
                    return new JsonResult(new { success = false, message = "No se pudo identificar al usuario" });
                }

                // Crear HTTP client
                var client = _clientFactory.CreateClient();
                
                // Obtener el token de autenticación
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                // Get the base URL from configuration
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                // Llamada a la API para crear el ítem
                _logger.LogInformation($"Enviando solicitud a: {baseUrl}/api/inventario/{userEmail}");
                _logger.LogInformation($"Datos del ítem: {System.Text.Json.JsonSerializer.Serialize(newItem)}");

                // Hacer la solicitud POST a la API
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(newItem),
                    System.Text.Encoding.UTF8,
                    "application/json");
                    
                var response = await client.PostAsync($"{baseUrl}/api/inventario/{userEmail}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Ítem creado correctamente");
                    return new JsonResult(new { success = true, message = "Ítem creado correctamente" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error al crear el ítem. API devolvió {response.StatusCode}: {errorContent}");
                    return new JsonResult(new { 
                        success = false, 
                        message = $"Error al crear el ítem. Código: {response.StatusCode}", 
                        details = errorContent 
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

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(updatedItem.Nombre))
                {
                    return new JsonResult(new { success = false, message = "El nombre del ítem es obligatorio" });
                }

                if (string.IsNullOrWhiteSpace(updatedItem.Categoria))
                {
                    return new JsonResult(new { success = false, message = "La categoría es obligatoria" });
                }

                if (updatedItem.Stock < 0)
                {
                    return new JsonResult(new { success = false, message = "El stock no puede ser negativo" });
                }

                // Obtener el correo del usuario desde las claims
                string userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("No se pudo obtener el correo de usuario de las claims");
                    return new JsonResult(new { success = false, message = "No se pudo identificar al usuario" });
                }

                // Crear HTTP client
                var client = _clientFactory.CreateClient();
                
                // Obtener el token de autenticación
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                // Get the base URL from configuration
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                // Llamada a la API para actualizar el ítem
                _logger.LogInformation($"Enviando solicitud PUT a: {baseUrl}/api/inventario/{userEmail}/{id}");
                _logger.LogInformation($"Datos de actualización: {JsonSerializer.Serialize(updatedItem)}");

                // Hacer la solicitud PUT a la API
                var content = new StringContent(
                    JsonSerializer.Serialize(updatedItem),
                    System.Text.Encoding.UTF8,
                    "application/json");
                    
                var response = await client.PutAsync($"{baseUrl}/api/inventario/{userEmail}/{id}", content);
                
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
            // In a real application, this would update via API
            _logger.LogInformation($"Updating stock for item {id} to {newStock}");
            
            // Simulate successful update
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

                // Obtener el correo del usuario desde las claims
                string userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogError("No se pudo obtener el correo de usuario de las claims");
                    return new JsonResult(new { success = false, message = "No se pudo identificar al usuario" });
                }

                // Crear HTTP client
                var client = _clientFactory.CreateClient();
                
                // Obtener el token de autenticación
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                // Get the base URL from configuration
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                // Llamada a la API para eliminar el ítem
                _logger.LogInformation($"Enviando solicitud DELETE a: {baseUrl}/api/inventario/{userEmail}/{id}");

                // Hacer la solicitud DELETE a la API
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
                // Crear HTTP client
                var client = _clientFactory.CreateClient();
                
                // Obtener el token de autenticación
                string token = User.Claims.FirstOrDefault(c => c.Type == "AccessToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                
                // Get the base URL from configuration
                string baseUrl = _configuration["ApiSettings:BaseUrl"];
                
                // Llamada a la API para obtener detalles del ítem específico
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