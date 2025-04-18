using GestorEventos.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gestor_eventos.Pages.Reservas
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PeriodFilter { get; set; }

        // Añadimos EventTypeFilter
        [BindProperty(SupportsGet = true)]
        public string EventTypeFilter { get; set; }

        // Añadimos DateFilter
        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        // Las propiedades StartDate y EndDate pueden mantenerse para uso interno
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }

        public List<Reservation> Reservations { get; set; }

        public void OnGet()
        {
            // Procesar PeriodFilter y convertirlo a fechas StartDate y EndDate
            ProcessPeriodFilter();

            // Obtener y filtrar reservas
            Reservations = GetSampleReservations();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Reservations = Reservations.Where(r => 
                    r.EventName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.ClientName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.ClientEmail.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                Reservations = Reservations.Where(r => r.Status == StatusFilter).ToList();
            }
            
            // Filtrar por tipo de evento
            if (!string.IsNullOrEmpty(EventTypeFilter))
            {
                Reservations = Reservations.Where(r => r.EventType == EventTypeFilter).ToList();
            }
            
            // Filtrar por fecha específica
            if (DateFilter.HasValue)
            {
                Reservations = Reservations.Where(r => r.Date.Date == DateFilter.Value.Date).ToList();
            }
            else
            {
                // Solo usamos StartDate y EndDate si DateFilter no está definido
                if (StartDate.HasValue)
                {
                    Reservations = Reservations.Where(r => r.Date >= StartDate.Value).ToList();
                }

                if (EndDate.HasValue)
                {
                    Reservations = Reservations.Where(r => r.Date <= EndDate.Value).ToList();
                }
            }
        }

        private void ProcessPeriodFilter()
        {
            if (string.IsNullOrEmpty(PeriodFilter))
                return;

            var today = DateTime.Today;

            switch (PeriodFilter)
            {
                case "today":
                    StartDate = today;
                    EndDate = today;
                    break;
                case "yesterday":
                    StartDate = today.AddDays(-1);
                    EndDate = today.AddDays(-1);
                    break;
                case "3days":
                    StartDate = today.AddDays(-3);
                    EndDate = today;
                    break;
                case "week":
                    StartDate = today.AddDays(-7);
                    EndDate = today;
                    break;
                case "month":
                    StartDate = today.AddMonths(-1);
                    EndDate = today;
                    break;
            }
        }

        private List<Reservation> GetSampleReservations()
        {
            // Este es solo un ejemplo de datos para fines de demostración
            return new List<Reservation>
            {
                new Reservation
                {
                    Id = 1,
                    EventName = "Boda de Carlos y María",
                    ClientName = "Carlos Rodríguez",
                    ClientEmail = "carlos@ejemplo.com",
                    ServiceName = "Boda Completa",
                    EventType = "Boda",
                    Date = new DateTime(2025, 4, 18),
                    Time = "16:00",
                    Amount = 1540000,
                    Status = "Confirmada",
                    Description = "Boda para 150 personas en Salón Diamante"
                },
                new Reservation
                {
                    Id = 2,
                    EventName = "Cumpleaños de Lucía",
                    ClientName = "María González",
                    ClientEmail = "maria@ejemplo.com",
                    ServiceName = "Cumpleaños",
                    EventType = "Cumpleaños",
                    Date = new DateTime(2025, 4, 22),
                    Time = "19:30",
                    Amount = 850000,
                    Status = "Pendiente",
                    Description = "Cumpleaños para 50 personas, requiere decoración temática"
                },
                new Reservation
                {
                    Id = 3,
                    EventName = "Tech Summit 2025",
                    ClientName = "Empresa ABC",
                    ClientEmail = "eventos@abc.com",
                    ServiceName = "Conferencia Corporativa",
                    EventType = "Corporativo",
                    Date = new DateTime(2025, 5, 5),
                    Time = "09:00",
                    Amount = 2350000,
                    Status = "Confirmada",
                    Description = "Conferencia de tecnología para 200 asistentes"
                },
                new Reservation
                {
                    Id = 4,
                    EventName = "Graduación Universidad Central",
                    ClientName = "Juan Pérez",
                    ClientEmail = "juan@ejemplo.com",
                    ServiceName = "Fiesta de Graduación",
                    EventType = "Graduación",
                    Date = new DateTime(2025, 5, 15),
                    Time = "20:00",
                    Amount = 1280000,
                    Status = "Cancelada",
                    Description = "Evento cancelado por el cliente"
                },
                new Reservation
                {
                    Id = 5,
                    EventName = "Bautizo de Matías",
                    ClientName = "Ana López",
                    ClientEmail = "ana@ejemplo.com",
                    ServiceName = "Bautizo",
                    EventType = "Bautizo",
                    Date = new DateTime(2025, 4, 30),
                    Time = "11:30",
                    Amount = 980000,
                    Status = "Pendiente",
                    Description = "Bautizo con recepción para 80 personas"
                },
                new Reservation
                {
                    Id = 6,
                    EventName = "25 Aniversario",
                    ClientName = "Roberto Díaz",
                    ClientEmail = "roberto@ejemplo.com",
                    ServiceName = "Aniversario",
                    EventType = "Aniversario",
                    Date = new DateTime(2025, 5, 10),
                    Time = "20:30",
                    Amount = 1220000,
                    Status = "Confirmada",
                    Description = "Cena de aniversario para 40 personas"
                },
                new Reservation
                {
                    Id = 7,
                    EventName = "Lanzamiento Producto Z-1000",
                    ClientName = "Empresa XYZ",
                    ClientEmail = "marketing@xyz.com",
                    ServiceName = "Lanzamiento de Producto",
                    EventType = "Corporativo",
                    Date = new DateTime(2025, 5, 20),
                    Time = "17:00",
                    Amount = 1842000,
                    Status = "Pendiente",
                    Description = "Presentación de nuevo producto con cóctel para 120 invitados"
                },
                new Reservation
                {
                    Id = 8,
                    EventName = "Conferencia Anual de Medicina",
                    ClientName = "Hospital Central",
                    ClientEmail = "eventos@hospitalcentral.com",
                    ServiceName = "Conferencia",
                    EventType = "Corporativo",
                    Date = new DateTime(2025, 6, 5),
                    Time = "08:30",
                    Amount = 2750000,
                    Status = "Finalizada",
                    Description = "Conferencia médica con participación internacional"
                }
            };
        }
    }

    public class Reservation
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal Amount { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ServiceName { get; set; } // Mantener para compatibilidad con código existente
    }
}
