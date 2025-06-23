using System.Collections.Generic;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Models; // Add this if InventarioItemApi is in gestor_eventos.Models
using gestor_eventos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace gestor_eventos.Pages.Servicios
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ServicioService _servicioService;
        private readonly InventoryService _inventoryService;

        public List<ServicioApi> Servicios { get; set; } = new List<ServicioApi>();
        public List<InventarioItemApi> InventarioItems { get; set; } = new List<InventarioItemApi>();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public IndexModel(
            ILogger<IndexModel> logger, 
            ServicioService servicioService, 
            InventoryService inventoryService)
        {
            _logger = logger;
            _servicioService = servicioService;
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync(string? success = null)
        {
            _logger.LogInformation("Cargando página de servicios");
            
            if (!string.IsNullOrEmpty(success))
            {
                SuccessMessage = success;
            }
            
            try
            {
                await LoadData();
            }
            catch (System.Exception ex)
            {
                HasError = true;
                ErrorMessage = "Ocurrió un error al cargar los servicios. Intente de nuevo más tarde.";
                _logger.LogError(ex, "Error al cargar servicios: {Message}", ex.Message);
            }
        }

        private async Task LoadData()
        {
            var serviciosTask = _servicioService.GetServiciosAsync();
            var inventarioTask = _inventoryService.GetInventoryItemsAsync();
            
            await Task.WhenAll(serviciosTask, inventarioTask);
            
            Servicios = await serviciosTask;
            InventarioItems = await inventarioTask;
            
            _logger.LogInformation("Cargados {ServiceCount} servicios y {ItemCount} ítems de inventario", 
                Servicios.Count, InventarioItems.Count);
        }

        public async Task<IActionResult> OnPostAsync([FromBody] ServicioCreateModel model)
        {
            _logger.LogInformation("Recibida solicitud para crear servicio: {NombreServicio}", model.NombreServicio);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }
            
            try
            {
                var result = await _servicioService.CreateServicioAsync(model);
                
                if (result == null)
                {
                    _logger.LogWarning("No se pudo crear el servicio");
                    return StatusCode(500, new { message = "Error al crear el servicio" });
                }
                
                _logger.LogInformation("Servicio creado exitosamente con ID: {Id}", result.Id);
                return new JsonResult(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEditAsync(string id, [FromBody] ServicioEditModel model)
        {
            _logger.LogInformation("Recibida solicitud para editar servicio: {Id}", id);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }
            
            try
            {
                var result = await _servicioService.UpdateServicioAsync(id, model);
                
                if (result == null)
                {
                    _logger.LogWarning("No se pudo editar el servicio");
                    return StatusCode(500, new { message = "Error al editar el servicio" });
                }
                
                _logger.LogInformation("Servicio editado exitosamente con ID: {Id}", result.Id);
                return new JsonResult(result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error al editar servicio: {Message}", ex.Message);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetServicesListAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de servicios para selector");
                var servicios = await _servicioService.GetServiciosAsync();
                
                if (servicios == null)
                {
                    _logger.LogWarning("No se pudieron obtener los servicios");
                    return new JsonResult(new { success = false, message = "No se pudieron obtener los servicios" });
                }
                
                return new JsonResult(new { success = true, data = servicios });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de servicios: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error al obtener la lista de servicios" });
            }
        }
    }
}