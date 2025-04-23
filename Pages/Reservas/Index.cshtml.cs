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

namespace gestor_eventos.Pages.Reservas
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ReservacionService _reservacionService;

        public IndexModel(ILogger<IndexModel> logger, ReservacionService reservacionService)
        {
            _logger = logger;
            _reservacionService = reservacionService;
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
                    return;
                }
                
                _logger.LogInformation($"Obteniendo reservaciones para el usuario: {userEmail}");
                
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
                _logger.LogError(ex, "Error al obtener las reservaciones");
                Reservaciones = new List<ReservacionApi>();
                Reservations = new List<Reservation>();
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
                Id = r.Id.GetHashCode() % 10000, // ID numérico simplificado
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
                ClientPhone = r.TelefonoCliente
            };
        }

        public class Reservation
        {
            public int Id { get; set; }
            public string EventName { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }
            public string ClientPhone { get; set; }  // Nueva propiedad para el teléfono
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public decimal Amount { get; set; }
            public string EventType { get; set; }
            public string Status { get; set; }
            public string Description { get; set; }
            public string ServiceName { get; set; }
        }
    }
}
