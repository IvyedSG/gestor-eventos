using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Services;
using GestorEventos.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using GestorEventos.Services;
using GestorEventos.Models.ApiModels;

namespace gestor_eventos.Pages.Reservas
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ReservacionService _reservacionService;
        private readonly ServicioService _servicioService;
        private readonly ClienteService _clienteService;

        public IndexModel(ILogger<IndexModel> logger, 
                         ReservacionService reservacionService,
                         ServicioService servicioService,
                         ClienteService clienteService)
        {
            _logger = logger;
            _reservacionService = reservacionService;
            _servicioService = servicioService;
            _clienteService = clienteService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string EventTypeFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodFilter { get; set; }

        public List<ReservacionApi> Reservaciones { get; set; }

        // Para compatibilidad con el código existente mientras hacemos la transición
        public List<Reservation> Reservations { get; set; }

        // Add this property
        public List<ServicioApi> ServiciosDisponibles { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener correo del usuario de los claims (forma más confiable)
                var userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                
                // Si no hay email en los claims, buscar en los claims con formato estándar
                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                // Validar que tenemos un correo electrónico
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo obtener el correo electrónico del usuario autenticado");
                    Reservaciones = new List<ReservacionApi>();
                    Reservations = new List<Reservation>();
                    ServiciosDisponibles = new List<ServicioApi>();
                    return;
                }
                
                _logger.LogInformation($"Obteniendo reservaciones para el usuario: {userEmail}");
                
                // Obtener servicios disponibles usando el nuevo método
                ServiciosDisponibles = await _servicioService.GetServiciosByCorreoAsync(userEmail);
                
                // Obtener reservaciones del API
                var apiReservaciones = await _reservacionService.GetReservacionesByCorreoAsync(userEmail);
                Reservaciones = apiReservaciones.ToList();
                
                // Aplicar filtros a las reservaciones
                AplicarFiltros();
                
                // Convertir ReservacionApi a Reservation para mantener compatibilidad con la vista existente
                Reservations = Reservaciones.Select(ConvertirAReservation).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las reservaciones o servicios: {Message}", ex.Message);
                Reservaciones = new List<ReservacionApi>();
                Reservations = new List<Reservation>();
                ServiciosDisponibles = new List<ServicioApi>();
            }
        }

        private void AplicarFiltros()
        {
            // Filtrar reservaciones según los criterios
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Reservaciones = Reservaciones.Where(r => 
                    (r.NombreEvento?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.NombreCliente?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.CorreoCliente?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Descripcion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                Reservaciones = Reservaciones.Where(r => r.Estado?.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }
            
            if (!string.IsNullOrEmpty(EventTypeFilter))
            {
                Reservaciones = Reservaciones.Where(r => r.TipoEvento?.Equals(EventTypeFilter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }
            
            if (DateFilter.HasValue)
            {
                Reservaciones = Reservaciones.Where(r => r.FechaEvento.Date == DateFilter.Value.Date).ToList();
            }
        }

        private Reservation ConvertirAReservation(ReservacionApi r)
        {
            return new Reservation
            {
                Id = r.Id.GetHashCode() % 10000, // ID numérico simplificado para mostrar
                OriginalId = r.Id.ToString(), // Guardar el ID original como string
                EventName = r.NombreEvento,
                ClientName = r.NombreCliente,
                ClientEmail = r.CorreoCliente,
                Date = r.FechaEvento,
                Time = r.HoraEvento,
                Amount = r.PrecioTotal,
                EventType = r.TipoEvento,
                Status = r.Estado,
                Description = r.Descripcion,
                ServiceName = r.ServicioId.ToString(),
                ClientPhone = r.TelefonoCliente,
                Services = r.Servicios?.Select(s => new ReservationService
                {
                    ServiceId = s.ServicioId,
                    Name = s.NombreServicio,
                    Quantity = s.CantidadItems,
                    Price = s.Precio
                }).ToList() ?? new List<ReservationService>()
            };
        }

        public class Reservation
        {
            public int Id { get; set; } // Simplified display ID
            public string OriginalId { get; set; } // Original GUID string ID
            public string EventName { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }
            public string ClientPhone { get; set; }
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public decimal Amount { get; set; }
            public string EventType { get; set; }
            public string Status { get; set; }
            public string Description { get; set; }
            public string ServiceName { get; set; }
            public List<ReservationService> Services { get; set; } = new List<ReservationService>();
        }

        public class ReservationService
        {
            public Guid ServiceId { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostUpdateReservationAsync([FromBody] ReservationUpdateRequest request)
        {
            try
            {
                // Validar entrada
                if (request == null || string.IsNullOrEmpty(request.id) || request.updateModel == null)
                {
                    return BadRequest("Datos de actualización inválidos");
                }

                if (!Guid.TryParse(request.id, out Guid reservaId))
                {
                    return BadRequest("El ID de reserva proporcionado no es válido");
                }

                // Obtener correo del usuario autenticado
                var userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                
                // Alternativa usando el tipo de claim estándar
                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo obtener el correo electrónico del usuario para actualizar la reservación");
                    return BadRequest("No se pudo identificar el usuario");
                }

                // Normalizar el estado antes de enviarlo a la API
                request.updateModel.estado = NormalizarEstado(request.updateModel.estado);

                // Asegurar que itemsToRemove sea una lista inicializada y válida
                if (request.updateModel.itemsToRemove == null)
                {
                    request.updateModel.itemsToRemove = new List<string>();
                }
                
                // Eliminar cualquier string vacío de itemsToRemove
                request.updateModel.itemsToRemove = request.updateModel.itemsToRemove
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                // Llamar al servicio para actualizar la reservación
                var resultado = await _reservacionService.UpdateReservacionAsync(userEmail, reservaId, request.updateModel);
                
                if (!resultado)
                {
                    _logger.LogWarning("No se pudo actualizar la reservación {Id} para el usuario {Email}", request.id, userEmail);
                    return BadRequest("No se pudo actualizar la reservación. Por favor, inténtelo de nuevo.");
                }

                return new JsonResult(new { success = true, message = "Reservación actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la reservación: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor al procesar la solicitud");
            }
        }

        // Clase para recibir los datos de la solicitud JSON
        public class ReservationUpdateRequest
        {
            public string id { get; set; }
            public ReservacionUpdateModel updateModel { get; set; }
        }

        // Método auxiliar para normalizar el estado según los valores permitidos
        private string NormalizarEstado(string estado)
        {
            if (string.IsNullOrEmpty(estado)) return "PENDIENTE";
            
            string estadoUpper = estado.ToUpper();
            
            if (estadoUpper.Contains("CONFIRM")) return "CONFIRMADO";
            if (estadoUpper.Contains("CANCEL")) return "CANCELADO";
            if (estadoUpper.Contains("FINAL") || estadoUpper.Contains("COMPLET")) return "FINALIZADO";
            
            // Por defecto
            return "PENDIENTE";
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnGetGetClientsAsync()
        {
            try
            {
                // Get user email from claims
                var userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo obtener el correo electrónico del usuario para obtener clientes");
                    return new JsonResult(new List<ClienteApi>());
                }
                
                var clients = await _clienteService.GetClientesByUsuarioAsync(userEmail);
                return new JsonResult(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los clientes: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor al obtener los clientes");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> OnPostCreateReservationAsync([FromBody] ReservacionCreateModel model)
        {
            try
            {
                // Validar entrada
                if (model == null)
                {
                    return BadRequest("Datos de creación inválidos");
                }

                // Obtener correo del usuario autenticado
                var userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                
                // Alternativa usando el tipo de claim estándar
                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo obtener el correo electrónico del usuario para crear la reservación");
                    return BadRequest("No se pudo identificar el usuario");
                }

                // Normalizar el estado antes de enviarlo a la API
                model.estado = NormalizarEstado(model.estado);

                // Asegurar que servicios sea una lista inicializada y válida
                if (model.servicios == null)
                {
                    model.servicios = new List<string>();
                }
                
                // Eliminar cualquier string vacío de servicios
                model.servicios = model.servicios
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();

                // Llamar al servicio para crear la reservación
                var (success, message) = await _reservacionService.CreateReservacionAsync(userEmail, model);
                
                if (!success)
                {
                    _logger.LogWarning("No se pudo crear la reservación para el usuario {Email}: {Message}", userEmail, message);
                    return BadRequest($"No se pudo crear la reservación: {message}");
                }

                return new JsonResult(new { success = true, message = "Reserva creada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la reservación: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor al procesar la solicitud");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnDeleteDeleteReservationAsync(string id)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("ID de reserva inválido");
                }

                // Verificar si el ID es un GUID válido
                if (!Guid.TryParse(id, out Guid reservaId))
                {
                    return BadRequest("El ID de reserva proporcionado no es válido");
                }

                // Obtener correo del usuario autenticado
                var userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;
                
                // Alternativa usando el tipo de claim estándar
                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo obtener el correo electrónico del usuario para eliminar la reservación");
                    return BadRequest("No se pudo identificar el usuario");
                }

                // Llamar al servicio para eliminar la reservación
                var result = await _reservacionService.DeleteReservacionAsync(userEmail, reservaId);
                
                if (!result)
                {
                    _logger.LogWarning("No se pudo eliminar la reservación {Id} para el usuario {Email}", id, userEmail);
                    return BadRequest("No se pudo eliminar la reservación. Por favor, inténtelo de nuevo.");
                }

                return new JsonResult(new { success = true, message = "Reservación eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la reservación: {Message}", ex.Message);
                return StatusCode(500, "Error interno del servidor al procesar la solicitud");
            }
        }
    }
}
