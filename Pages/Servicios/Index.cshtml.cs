using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using gestor_eventos.Services;
using gestor_eventos.Models.ApiModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace gestor_eventos.Pages.Servicios
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ServicioService _servicioService;
        private readonly InventoryService _inventoryService;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string EventTypeFilter { get; set; } // Cambiado de CategoryFilter a EventTypeFilter

        public List<ServiceModel> Services { get; set; } = new List<ServiceModel>();

        // Add this property to store inventory items
        public List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

        // Update the constructor to inject the InventoryService
        public IndexModel(
            ServicioService servicioService, 
            InventoryService inventoryService,
            ILogger<IndexModel> logger)
        {
            _servicioService = servicioService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Get the current user's email and ID
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userId = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User email or ID not found in claims");
                    return;
                }

                // Load services for the user
                var serviciosApi = await _servicioService.GetServiciosByCorreoAsync(userEmail);
                Services = ConvertToServiceModels(serviciosApi);

                // Apply filters if they exist
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    Services = Services.Where(s => 
                        s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(EventTypeFilter))
                {
                    Services = Services.Where(s => 
                        s.EventTypes.Contains(EventTypeFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Load inventory items for the user
                InventoryItems = await _inventoryService.GetInventoryItemsByUserIdAsync(userId);
                _logger.LogInformation("Loaded {ItemCount} inventory items for user", InventoryItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading page data: {Message}", ex.Message);
            }
        }

        private List<ServiceModel> ConvertToServiceModels(List<ServicioApi> serviciosApi)
        {
            var result = new List<ServiceModel>();

            foreach (var servicio in serviciosApi)
            {
                // Convertir cada servicio del API a ServiceModel
                var serviceModel = new ServiceModel
                {
                    Id = servicio.Id.ToString(),
                    Name = servicio.NombreServicio,
                    Description = servicio.Descripcion,
                    BasePrice = servicio.PrecioBase,
                    EventTypes = servicio.TipoEvento,
                    
                    // Procesar las imágenes si existen, o usar una imagen por defecto
                    MainImage = !string.IsNullOrEmpty(servicio.Imagenes) ? 
                        servicio.Imagenes.Split(',').First() : 
                        "/assets/img/placeholder-img.png",
                    
                    Images = !string.IsNullOrEmpty(servicio.Imagenes) ? 
                        servicio.Imagenes.Split(',').ToList() : 
                        new List<string> { "/assets/img/placeholder-img.png" },
                        
                    // Mapear los items
                    Items = servicio.Items?.Select(item => new ServiceItemModel
                    {
                        Id = item.Id.ToString(),
                        InventarioId = item.InventarioId.ToString(),
                        Cantidad = item.Cantidad,
                        NombreItem = item.NombreItem,
                        CategoriaItem = item.CategoriaItem,
                        StockActual = item.StockActual
                    }).ToList() ?? new List<ServiceItemModel>(),
                    
                    TotalItems = servicio.TotalItems
                };

                result.Add(serviceModel);
            }

            return result;
        }
    }

    public class ServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string EventTypes { get; set; }
        public string MainImage { get; set; }
        public List<string> Images { get; set; }
        public List<ServiceItemModel> Items { get; set; } = new List<ServiceItemModel>();
        public int TotalItems { get; set; }
    }

    public class ServiceItemModel
    {
        public string Id { get; set; }
        public string InventarioId { get; set; }
        public int Cantidad { get; set; }
        public string NombreItem { get; set; }
        public string CategoriaItem { get; set; }
        public int StockActual { get; set; }
    }
}