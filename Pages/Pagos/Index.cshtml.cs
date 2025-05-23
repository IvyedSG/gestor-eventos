using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using gestor_eventos.Services;
using gestor_eventos.Models.ApiModels;
using GestorEventos.Filters;
using System.Linq;
using System.Security.Claims;

namespace gestor_eventos.Pages.Pagos
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PagoService _pagoService;
        private readonly ReservacionService _reservacionService;

        public IndexModel(ILogger<IndexModel> logger, PagoService pagoService, ReservacionService reservacionService)
        {
            _logger = logger;
            _pagoService = pagoService;
            _reservacionService = reservacionService;
        }

        [BindProperty(SupportsGet = true)]
        public string ReservaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TipoPagoFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPagos { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal IngresosMes { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalPagos / (double)PageSize);

        public List<PagoDto> Pagos { get; set; } = new List<PagoDto>();
        public List<PagoDto> Payments => Pagos; // Alias para mantener compatibilidad con las vistas

        // Lista de tipos de pago disponibles como objetos para mantener compatibilidad con la vista
        public List<TipoPagoModel> TiposPago { get; set; } = new List<TipoPagoModel>
        { 
            new TipoPagoModel { Id = "EFECTIVO", Nombre = "Efectivo" },
            new TipoPagoModel { Id = "TARJETA", Nombre = "Tarjeta" },
            new TipoPagoModel { Id = "TRANSFERENCIA", Nombre = "Transferencia" },
            new TipoPagoModel { Id = "YAPE", Nombre = "Yape" },
            new TipoPagoModel { Id = "PLIN", Nombre = "Plin" },
            new TipoPagoModel { Id = "OTRO", Nombre = "Otro" }
        };

        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Si se especifica un ID de reserva, mostrar solo los pagos de esa reserva
                if (!string.IsNullOrEmpty(ReservaId))
                {
                    Pagos = await _pagoService.GetPagosByReservaAsync(ReservaId);
                }
                else
                {
                    // Obtener todos los pagos del usuario
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    if (string.IsNullOrEmpty(userEmail))
                    {
                        HasError = true;
                        ErrorMessage = "No se pudo identificar al usuario actual";
                        return;
                    }
                    
                    Pagos = await _pagoService.GetPagosByUsuarioAsync(userEmail);
                }

                // Completar información adicional de cada pago
                await CompletarInformacionPagos();

                // Aplicar filtros
                AplicarFiltros();

                // Calcular totales
                CalcularEstadisticas();

                // Aplicar paginación
                TotalPagos = Pagos.Count;
                Pagos = Pagos
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los pagos");
                HasError = true;
                ErrorMessage = "Error al cargar los pagos. Por favor, inténtalo de nuevo más tarde.";
                Pagos = new List<PagoDto>();
            }
        }

        private async Task CompletarInformacionPagos()
        {
            // Obtener IDs únicos de reservas
            var reservasIds = Pagos.Select(p => p.ReservaId.ToString()).Distinct().ToList();
            
            // Obtener información de cada reserva
            foreach (var reservaId in reservasIds)
            {
                try
                {
                    var reservaInfo = await _reservacionService.GetReservacionAsync(reservaId);
                    if (reservaInfo != null)
                    {
                        // Actualizar información en todos los pagos asociados a esta reserva
                        foreach (var pago in Pagos.Where(p => p.ReservaId.ToString() == reservaId))
                        {
                            pago.NombreReserva = reservaInfo.NombreEvento ?? "Reserva " + pago.ReservaId.ToString().Substring(0, 8);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"No se pudo obtener información de la reserva {reservaId}");
                }
            }

            // Establecer un nombre predeterminado para los pagos sin nombre de reserva
            foreach (var pago in Pagos.Where(p => string.IsNullOrEmpty(p.NombreReserva)))
            {
                pago.NombreReserva = "Reserva " + pago.ReservaId.ToString().Substring(0, 8);
            }
        }

        private void AplicarFiltros()
        {
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Pagos = Pagos.Where(p => 
                    (p.NombreReserva?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Descripcion?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.ReservaId.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            if (!string.IsNullOrEmpty(TipoPagoFilter))
            {
                Pagos = Pagos.Where(p => p.TipoPago == TipoPagoFilter).ToList();
            }

            if (DateFilter.HasValue)
            {
                Pagos = Pagos.Where(p => p.FechaPago.Date == DateFilter.Value.Date).ToList();
            }
        }

        private void CalcularEstadisticas()
        {
            TotalIngresos = Pagos.Sum(p => p.Monto);
            
            var hoy = DateTime.Today;
            var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
            
            IngresosMes = Pagos
                .Where(p => p.FechaPago >= primerDiaMes && p.FechaPago <= ultimoDiaMes)
                .Sum(p => p.Monto);
        }

        public class CreatePagoRequest
        {
            public string ReservaId { get; set; }
            public string TipoPago { get; set; }
            public decimal Monto { get; set; }
            public string Estado { get; set; }
            public string Descripcion { get; set; }
            public string ComprobanteUrl { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCreatePagoAsync([FromBody] CreatePagoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ReservaId) || 
                    string.IsNullOrEmpty(request.TipoPago) || 
                    request.Monto <= 0)
                {
                    return new JsonResult(new { success = false, message = "Los campos obligatorios no pueden estar vacíos y el monto debe ser mayor que cero" });
                }

                var pagoDto = new PagoDto
                {
                    ReservaId = Guid.Parse(request.ReservaId),
                    TipoPago = request.TipoPago,
                    Monto = request.Monto,
                    Estado = request.Estado ?? "PENDIENTE",
                    Descripcion = request.Descripcion ?? string.Empty,
                    ComprobanteUrl = request.ComprobanteUrl ?? string.Empty,
                    FechaPago = DateTime.Now
                };

                var result = await _pagoService.CreatePagoAsync(pagoDto);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo crear el pago" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
            }
        }

        public class UpdatePagoRequest
        {
            public string Id { get; set; }
            public string TipoPago { get; set; }
            public decimal Monto { get; set; }
            public string Estado { get; set; }
            public string Descripcion { get; set; }
            public string ComprobanteUrl { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdatePagoAsync([FromBody] UpdatePagoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Id) || 
                    string.IsNullOrEmpty(request.TipoPago) || 
                    request.Monto <= 0)
                {
                    return new JsonResult(new { success = false, message = "Los campos obligatorios no pueden estar vacíos y el monto debe ser mayor que cero" });
                }

                var pagoDto = new PagoDto
                {
                    Id = Guid.Parse(request.Id),
                    TipoPago = request.TipoPago,
                    Monto = request.Monto,
                    Estado = request.Estado ?? "PENDIENTE",
                    Descripcion = request.Descripcion ?? string.Empty,
                    ComprobanteUrl = request.ComprobanteUrl ?? string.Empty,
                    FechaPago = DateTime.Now // Consideración: podrías querer mantener la fecha original
                };

                var result = await _pagoService.UpdatePagoAsync(request.Id, pagoDto);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo actualizar el pago" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pago");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
            }
        }

        public class DeletePagoRequest
        {
            public string Id { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeletePagoAsync([FromBody] DeletePagoRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Id))
                {
                    return new JsonResult(new { success = false, message = "El ID del pago es requerido" });
                }

                var result = await _pagoService.DeletePagoAsync(request.Id);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo eliminar el pago" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
            }
        }

        public class TipoPagoModel
        {
            public string Id { get; set; }
            public string Nombre { get; set; }
        }
    }
}
