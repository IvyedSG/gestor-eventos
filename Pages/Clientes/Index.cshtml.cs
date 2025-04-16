using GestorEventos.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gestor_eventos.Pages.Clientes
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
        public string TypeFilter { get; set; }

        public List<Client> Clients { get; set; }

        public void OnGet()
        {
            // Obtener y filtrar clientes
            Clients = GetSampleClients();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Clients = Clients.Where(c => 
                    c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                    c.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                    c.Phone.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(TypeFilter))
            {
                Clients = Clients.Where(c => c.Type == TypeFilter).ToList();
            }
        }

        private List<Client> GetSampleClients()
        {
            // Datos de prueba
            return new List<Client>
            {
                new Client
                {
                    Id = 1,
                    Name = "Carlos Rodríguez",
                    Email = "carlos@ejemplo.com",
                    Phone = "987-654-321",
                    Type = "Individual",
                    EventCount = 3,
                    LastReservation = new DateTime(2025, 4, 18)
                },
                new Client
                {
                    Id = 2,
                    Name = "María González",
                    Email = "maria@ejemplo.com",
                    Phone = "987-123-456",
                    Type = "Individual",
                    EventCount = 1,
                    LastReservation = new DateTime(2025, 4, 22)
                },
                new Client
                {
                    Id = 3,
                    Name = "Empresa ABC",
                    Email = "contacto@abc.com",
                    Phone = "555-123-456",
                    Type = "Empresa",
                    EventCount = 5,
                    LastReservation = new DateTime(2025, 5, 5)
                },
                new Client
                {
                    Id = 4,
                    Name = "Juan Pérez",
                    Email = "juan@ejemplo.com",
                    Phone = "987-987-987",
                    Type = "Individual",
                    EventCount = 0,
                    LastReservation = null
                },
                new Client
                {
                    Id = 5,
                    Name = "Empresa XYZ",
                    Email = "info@xyz.com",
                    Phone = "555-789-123",
                    Type = "Empresa",
                    EventCount = 2,
                    LastReservation = new DateTime(2025, 5, 20)
                }
            };
        }
    }

    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }
        public int EventCount { get; set; }
        public DateTime? LastReservation { get; set; }
    }
}