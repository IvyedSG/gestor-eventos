using System;
using System.Collections.Generic;
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
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger, PagoService pagoService)
        {
            _logger = logger;
            _pagoService = pagoService;
        }

        public async Task OnGetAsync(string success = null)
        {
            _logger.LogInformation("Página de pagos cargada");

            if (!string.IsNullOrEmpty(success))
            {
                SuccessMessage = "La operación se completó exitosamente";
            }

            try
            {
                Pagos = await _pagoService.GetAllPagosAsync();
                Reservaciones = await _pagoService.GetAllReservacionesAsync();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Error al cargar los datos: " + ex.Message;
                _logger.LogError(ex, "Error al cargar datos de pagos");
            }
        }

        [HttpPost]
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

        [HttpPost]
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

        [HttpPost]
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