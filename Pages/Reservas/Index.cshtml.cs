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
                    r.ClientName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.ServiceName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    r.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                Reservations = Reservations.Where(r => r.Status == StatusFilter).ToList();
            }
            
            // Añadimos la lógica para filtrar por tipo de evento
            if (!string.IsNullOrEmpty(EventTypeFilter))
            {
                Reservations = Reservations.Where(r => r.ServiceName.Contains(EventTypeFilter)).ToList();
            }
            
            // Añadimos la lógica para filtrar por fecha específica
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
                    ClientName = "Carlos Rodríguez",
                    ServiceName = "Boda Completa",
                    Date = new DateTime(2025, 4, 18),
                    Time = "16:00",
                    Amount = 54000,
                    Status = "Confirmada",
                    Description = "Boda para 150 personas en Salón Diamante"
                },
                new Reservation
                {
                    Id = 2,
                    ClientName = "María González",
                    ServiceName = "Cumpleaños",
                    Date = new DateTime(2025, 4, 22),
                    Time = "19:30",
                    Amount = 15000,
                    Status = "Pendiente",
                    Description = "Cumpleaños para 50 personas, requiere decoración temática"
                },
                new Reservation
                {
                    Id = 3,
                    ClientName = "Empresa ABC",
                    ServiceName = "Conferencia Corporativa",
                    Date = new DateTime(2025, 5, 5),
                    Time = "09:00",
                    Amount = 35000,
                    Status = "Confirmada",
                    Description = "Conferencia de tecnología para 200 asistentes"
                },
                new Reservation
                {
                    Id = 4,
                    ClientName = "Juan Pérez",
                    ServiceName = "Fiesta de Graduación",
                    Date = new DateTime(2025, 5, 15),
                    Time = "20:00",
                    Amount = 28000,
                    Status = "Cancelada",
                    Description = "Evento cancelado por el cliente"
                },
                new Reservation
                {
                    Id = 5,
                    ClientName = "Ana López",
                    ServiceName = "Bautizo",
                    Date = new DateTime(2025, 4, 30),
                    Time = "11:30",
                    Amount = 18000,
                    Status = "Pendiente",
                    Description = "Bautizo con recepción para 80 personas"
                },
                new Reservation
                {
                    Id = 6,
                    ClientName = "Roberto Díaz",
                    ServiceName = "Aniversario",
                    Date = new DateTime(2025, 5, 10),
                    Time = "20:30",
                    Amount = 22000,
                    Status = "Confirmada",
                    Description = "Cena de aniversario para 40 personas"
                },
                new Reservation
                {
                    Id = 7,
                    ClientName = "Empresa XYZ",
                    ServiceName = "Lanzamiento de Producto",
                    Date = new DateTime(2025, 5, 20),
                    Time = "17:00",
                    Amount = 42000,
                    Status = "Pendiente",
                    Description = "Presentación de nuevo producto con cóctel para 120 invitados"
                }
            };
        }
    }

    public class Reservation
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}
