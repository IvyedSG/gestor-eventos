using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Services;
using GestorEventos.Models.ApiModels;
using GestorEventos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace gestor_eventos.Pages.Reservas
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ReservacionService _reservacionService;
        private readonly ClienteService _clienteService;
        private readonly ServicioService _servicioService;
        private readonly PagoService _pagoService;

        public List<ReservacionApi> Reservations { get; set; } = new List<ReservacionApi>();
        public List<ClienteApi> Clientes { get; set; } = new List<ClienteApi>();
        public List<ServicioApi> Servicios { get; set; } = new List<ServicioApi>();
        
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        // Agregar propiedades para los filtros
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string DateFilter { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            ReservacionService reservacionService,
            ClienteService clienteService,
            ServicioService servicioService,
            PagoService pagoService)
        {
            _logger = logger;
            _reservacionService = reservacionService;
            _clienteService = clienteService;
            _servicioService = servicioService;
            _pagoService = pagoService;
        }

        public async Task OnGetAsync(string success = null)
        {
            _logger.LogInformation("Página de reservas cargada");

            if (!string.IsNullOrEmpty(success))
            {
                SuccessMessage = "La operación se completó exitosamente";
            }

            try
            {
                await LoadData();
                // Cargar datos necesarios para el modal de creación
                Clientes = await _clienteService.GetClientesByUsuarioAsync(User.FindFirst("Email")?.Value ?? "");
                Servicios = await _servicioService.GetServiciosAsync();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "Error al cargar los datos: " + ex.Message;
                _logger.LogError(ex, "Error al cargar datos de reservas");
            }
        }

        [BindProperty]
        public ReservacionCreateModel NewReservacion { get; set; } = new ReservacionCreateModel();

        [HttpPost]
        public async Task<IActionResult> OnPostAsync([FromBody]ReservacionCreateModel model = null)
        {
            try
            {
                _logger.LogInformation("OnPostAsync called with model: {ModelNull}", model == null ? "null" : "not null");
                
                // Use the model if provided, otherwise use NewReservacion
                var reservationToCreate = model ?? NewReservacion;
                
                _logger.LogInformation("Creando nueva reservación: {NombreEvento}", reservationToCreate.NombreEvento);
                _logger.LogInformation("Datos de reservación: {@Reservation}", reservationToCreate);
                
                var result = await _reservacionService.CreateReservacionAsync(reservationToCreate);
                
                if (result == null)
                {
                    _logger.LogWarning("Error al crear la reservación");
                    return StatusCode(500, new { success = false, message = "Error al crear la reservación" });
                }
                
                _logger.LogInformation("Reservación creada exitosamente: {Id}", result.Id);
                return new JsonResult(new { success = true, message = "Reservación creada exitosamente", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Asegúrate de que este método funcione correctamente
        public async Task<IActionResult> OnGetReservationDetailsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "ID de reserva no proporcionado" });
            }

            try
            {
                _logger.LogInformation("Obteniendo detalles de reserva con ID: {Id}", id);
                var reservation = await _reservacionService.GetReservacionByIdAsync(id);
                
                if (reservation == null)
                {
                    _logger.LogWarning("Reserva no encontrada con ID: {Id}", id);
                    return NotFound(new { success = false, message = "Reserva no encontrada" });
                }
                
                // Log para verificar qué datos se están devolviendo
                _logger.LogInformation("Datos de reserva obtenidos: {@Reservation}", reservation);
                
                return new JsonResult(new { success = true, data = reservation });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de la reserva: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error al obtener los detalles de la reserva" });
            }
        }

        public async Task<IActionResult> OnPostUpdateReservationAsync([FromBody] ReservacionUpdateModel model, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "ID de reserva no proporcionado" });
            }

            try
            {
                _logger.LogInformation("Actualizando reserva con ID: {Id}", id);
                
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "No se proporcionaron datos para actualizar" });
                }

                // Log detallado para debugging
                _logger.LogInformation("Datos recibidos para actualización: {@Model}", model);

                // No es necesario verificar tipoEventoId porque ahora usamos tipoEventoNombre
                if (string.IsNullOrEmpty(model.tipoEventoNombre))
                {
                    _logger.LogWarning("tipoEventoNombre es null o vacío");
                }
                
                if (string.IsNullOrEmpty(model.servicioId))
                {
                    _logger.LogWarning("servicioId es null o vacío");
                }

                // La fecha ya debe venir en el formato correcto desde el cliente

                var result = await _reservacionService.UpdateReservacionAsync(id, model);
                
                if (result == null)
                {
                    _logger.LogWarning("Error al actualizar la reserva con ID: {Id}", id);
                    return StatusCode(500, new { success = false, message = "Error al actualizar la reserva" });
                }
                
                _logger.LogInformation("Reserva actualizada exitosamente: {Id}", result.Id);
                return new JsonResult(new { success = true, message = "Reserva actualizada exitosamente", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar reserva: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error detallado: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostDeleteReservationAsync([FromBody] DeleteReservationRequest request)
        {
            if (string.IsNullOrEmpty(request?.Id))
            {
                return BadRequest(new { success = false, message = "ID de reserva no proporcionado" });
            }

            try
            {
                _logger.LogInformation("Eliminando reserva con ID: {Id}", request.Id);
                
                var result = await _reservacionService.DeleteReservacionAsync(request.Id);
                
                if (!result)
                {
                    _logger.LogWarning("Error al eliminar la reserva con ID: {Id}", request.Id);
                    return StatusCode(500, new { success = false, message = "Error al eliminar la reserva" });
                }
                
                _logger.LogInformation("Reserva eliminada exitosamente: {Id}", request.Id);
                return new JsonResult(new { success = true, message = "Reserva eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reserva: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public class DeleteReservationRequest
        {
            public string Id { get; set; }
        }

        private async Task LoadData()
        {
            var allReservations = await _reservacionService.GetAllReservacionesAsync();

            // Aplicar filtros si existen
            var filteredReservations = allReservations;

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                filteredReservations = filteredReservations.Where(r =>
                    (r.NombreEvento?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.NombreCliente?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CorreoCliente?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Id?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                filteredReservations = filteredReservations.Where(r =>
                    r.Estado?.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase) ?? false
                ).ToList();
            }

            if (!string.IsNullOrEmpty(DateFilter))
            {
                if (DateTime.TryParse(DateFilter, out DateTime dateValue))
                {
                    filteredReservations = filteredReservations.Where(r =>
                        r.FechaEjecucion.Date == dateValue.Date
                    ).ToList();
                }
            }

            Reservations = filteredReservations;
        }

        // Add this new handler method
        public async Task<IActionResult> OnGetReservationPaymentsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "ID de reserva no proporcionado" });
            }

            try
            {
                _logger.LogInformation("Obteniendo pagos para la reserva con ID: {Id}", id);
                var payments = await _pagoService.GetPagosForReservacionAsync(id);
                
                return new JsonResult(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos de la reserva: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error al obtener los pagos de la reserva" });
            }
        }
    }
}