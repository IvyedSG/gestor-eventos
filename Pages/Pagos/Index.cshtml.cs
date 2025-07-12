using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace gestor_eventos.Pages.Pagos
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PagoService _pagoService;

        public List<PagoApi> Pagos { get; set; } = new List<PagoApi>();
        public List<ReservacionApi> Reservaciones { get; set; } = new List<ReservacionApi>();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        // Propiedades para filtros
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string TypeFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; } = string.Empty;

        // Propiedades para paginación
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public IndexModel(ILogger<IndexModel> logger, PagoService pagoService)
        {
            _logger = logger;
            _pagoService = pagoService;
        }

        public async Task OnGetAsync(string? success = null)
        {
            _logger.LogInformation("Página de pagos cargada");

            if (!string.IsNullOrEmpty(success))
            {
                SuccessMessage = "La operación se completó exitosamente";
            }

            try
            {
                await LoadData();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Error al cargar los datos: " + ex.Message;
                _logger.LogError(ex, "Error al cargar datos de pagos");
            }
        }

        private async Task LoadData()
        {
            var allPagos = await _pagoService.GetAllPagosAsync();
            Reservaciones = await _pagoService.GetAllReservacionesAsync();

            // Aplicar filtros
            var filteredPagos = allPagos;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredPagos = filteredPagos.Where(p =>
                    (p.NombreReserva?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Id?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.TipoPagoNombre?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(TypeFilter))
            {
                filteredPagos = filteredPagos.Where(p =>
                    p.TipoPagoNombre?.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase) ?? false
                ).ToList();
            }

            if (!string.IsNullOrEmpty(DateFilter))
            {
                if (DateTime.TryParse(DateFilter, out DateTime dateValue))
                {
                    filteredPagos = filteredPagos.Where(p =>
                        p.FechaPago.Date == dateValue.Date
                    ).ToList();
                }
            }

            // Calcular totales para paginación
            TotalCount = filteredPagos.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Asegurar que CurrentPage esté dentro del rango válido
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Aplicar paginación
            Pagos = filteredPagos
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            _logger.LogInformation("Loaded {Count} payments (Page {CurrentPage} of {TotalPages})", 
                Pagos.Count, CurrentPage, TotalPages);
        }

        public async Task<IActionResult> OnPostCreatePaymentAsync([FromBody] PagoCreateModel model)
        {
            try
            {
                _logger.LogInformation("Creando nuevo pago: {@Model}", model);
                
                if (string.IsNullOrEmpty(model.IdReserva))
                {
                    return BadRequest(new { success = false, message = "La reserva es obligatoria" });
                }
                
                if (string.IsNullOrEmpty(model.NombreTipoPago))
                {
                    return BadRequest(new { success = false, message = "El tipo de pago es obligatorio" });
                }
                
                if (string.IsNullOrEmpty(model.Monto) || !decimal.TryParse(model.Monto, out _))
                {
                    return BadRequest(new { success = false, message = "El monto debe ser un número válido" });
                }
                
                var result = await _pagoService.CreatePagoAsync(model);
                
                if (result == null)
                {
                    return StatusCode(500, new { success = false, message = "Error al crear el pago" });
                }
                
                return new JsonResult(new { success = true, message = "Pago creado exitosamente", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el pago: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostUpdatePaymentAsync([FromQuery] string id, [FromBody] PagoUpdateModel model)
        {
            try
            {
                _logger.LogInformation("Actualizando pago {Id}: {@Model}", id, model);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { success = false, message = "El ID del pago es obligatorio" });
                }
                
                if (string.IsNullOrEmpty(model.NombreTipoPago))
                {
                    return BadRequest(new { success = false, message = "El tipo de pago es obligatorio" });
                }
                
                if (string.IsNullOrEmpty(model.Monto) || !decimal.TryParse(model.Monto, out _))
                {
                    return BadRequest(new { success = false, message = "El monto debe ser un número válido" });
                }
                
                var result = await _pagoService.UpdatePagoAsync(id, model);
                
                if (result == null)
                {
                    return StatusCode(500, new { success = false, message = "Error al actualizar el pago" });
                }
                
                return new JsonResult(new { success = true, message = "Pago actualizado exitosamente", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el pago: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error al actualizar el pago" });
            }
        }

        public async Task<IActionResult> OnPostDeletePaymentAsync([FromQuery] string id)
        {
            try
            {
                _logger.LogInformation("Eliminando pago {Id}", id);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { success = false, message = "El ID del pago es obligatorio" });
                }
                
                var result = await _pagoService.DeletePagoAsync(id);
                
                if (!result)
                {
                    return StatusCode(500, new { success = false, message = "Error al eliminar el pago" });
                }
                
                return new JsonResult(new { success = true, message = "Pago eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el pago: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error al eliminar el pago" });
            }
        }
    }
}