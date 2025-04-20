using GestorEventos.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestorEventos.Services;
using GestorEventos.Models.ApiModels;
using System.Security.Claims;

namespace gestor_eventos.Pages.Clientes
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ClienteService _clienteService;

        public IndexModel(ILogger<IndexModel> logger, ClienteService clienteService)
        {
            _logger = logger;
            _clienteService = clienteService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TypeFilter { get; set; }

        public List<Client> Clients { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasToken { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener el correo del usuario actual desde las claims
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                // Verificar si tenemos token
                HasToken = User.FindFirst("AccessToken") != null;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    ErrorMessage = "No se pudo determinar el usuario actual";
                    Clients = new List<Client>();
                    return;
                }

                if (!HasToken)
                {
                    ErrorMessage = "No se ha podido autenticar con el API. Por favor, vuelve a iniciar sesión.";
                    Clients = new List<Client>();
                    return;
                }

                // Obtener los clientes de la API
                var apiClients = await _clienteService.GetClientesByUsuarioAsync(userEmail);
                
                if (apiClients.Count == 0)
                {
                    _logger.LogInformation("No se encontraron clientes para el usuario {Email}", userEmail);
                }
                
                // Convertir los clientes de la API al modelo local
                Clients = apiClients.Select(c => new Client
                {
                    Id = c.Id,
                    Name = c.Nombre,
                    Email = c.CorreoElectronico,
                    Phone = c.Telefono,
                    Address = c.Direccion,
                    Type = c.TipoCliente == "INDIVIDUAL" ? "Individual" : "Empresa",
                    EventCount = 0, // Este dato no viene de la API, se podría implementar luego
                    LastReservation = null, // Este dato no viene de la API, se podría implementar luego
                    RegistrationDate = c.FechaRegistro
                }).ToList();

                // Aplicar filtros si existen
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    Clients = Clients.Where(c => 
                        c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        c.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        c.Phone.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (c.Address != null && c.Address.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(TypeFilter))
                {
                    Clients = Clients.Where(c => c.Type.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los clientes");
                ErrorMessage = "Ocurrió un error al cargar los clientes. Por favor, inténtalo de nuevo más tarde.";
                Clients = new List<Client>();
            }
        }
    }

    public class Client
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public int EventCount { get; set; }
        public DateTime? LastReservation { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}