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
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public List<ServiceModel> Services { get; set; } = new List<ServiceModel>();

        public IndexModel(ServicioService servicioService, ILogger<IndexModel> logger)
        {
            _servicioService = servicioService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener el correo del usuario actual
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se encontró el email del usuario en las claims");
                    return;
                }

                // Obtener los servicios desde el API
                var serviciosApi = await _servicioService.GetServiciosByCorreoAsync(userEmail);

                // Convertir los datos del API al modelo de la vista
                Services = ConvertToServiceModels(serviciosApi);

                // Aplicar filtros si existen
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    Services = Services.Where(s => 
                        s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        s.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(CategoryFilter))
                {
                    Services = Services.Where(s => 
                        s.Category.Equals(CategoryFilter, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios: {Message}", ex.Message);
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
                    
                    // Determinar la categoría según los ítems o usar una por defecto
                    Category = DetermineServiceCategory(servicio),
                    
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

        private string DetermineServiceCategory(ServicioApi servicio)
        {
            // Lógica para determinar la categoría según los ítems o tipo de evento
            if (servicio.Items != null && servicio.Items.Any())
            {
                // Contar categorías para determinar la predominante
                var categories = servicio.Items
                    .GroupBy(i => i.CategoriaItem)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .ToList();

                if (categories.Any())
                {
                    // Mapear categorías del API a las categorías de la UI
                    switch (categories.First().ToLower())
                    {
                        case "alimentacion":
                        case "comida":
                            return "Catering";
                        case "decoracion":
                        case "adornos":
                            return "Decoración";
                        case "iluminacion":
                        case "sonido":
                        case "video":
                            return "Audio y Video";
                        case "muebles":
                        case "sillas":
                        case "mesas":
                            return "Mobiliario";
                        case "entretenimiento":
                        case "musica":
                            return "Entretenimiento";
                        default:
                            return "Otros";
                    }
                }
            }

            // Si no hay ítems, intentamos determinar por el tipo de evento
            switch (servicio.TipoEvento.ToLower())
            {
                case "boda":
                    return "Decoración";
                case "cumpleaños":
                    return "Entretenimiento";
                case "corporativo":
                    return "Audio y Video";
                default:
                    return "Otros";
            }
        }
    }

    public class ServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
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