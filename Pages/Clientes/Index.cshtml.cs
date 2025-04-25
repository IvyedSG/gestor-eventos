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
 
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
 
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

 
                var apiClients = await _clienteService.GetClientesByUsuarioAsync(userEmail);
                
                if (apiClients.Count == 0)
                {
                    _logger.LogInformation("No se encontraron clientes para el usuario {Email}", userEmail);
                }
                
 
                Clients = apiClients.Select(c => new Client
                {
                    Id = c.Id,
                    Name = c.Nombre,
                    Email = c.CorreoElectronico,
                    Phone = c.Telefono,
                    Address = c.Direccion,
                    Type = c.TipoCliente == "INDIVIDUAL" ? "Individual" : "Empresa",
                    EventCount = c.TotalReservas, // Usar el valor de la API
                    LastReservation = c.UltimaFechaReserva, // Usar el valor de la API
                    RegistrationDate = c.FechaRegistro
                }).ToList();

 
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

        public class ClienteCreateRequest
        {
            public string TipoCliente { get; set; }
            public string Nombre { get; set; }
            public string CorreoElectronico { get; set; }
            public string Telefono { get; set; }
            public string Direccion { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCreateClienteAsync([FromBody] ClienteCreateRequest request)
        {
            try
            {
 
                if (request == null)
                {
                    _logger.LogWarning("Request body es nulo");
                    return new JsonResult(new { success = false, message = "No se recibió información del cliente" });
                }

 
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo determinar el usuario actual");
                    return new JsonResult(new { success = false, message = "No se pudo determinar el usuario actual" });
                }

 
                if (User.FindFirst("AccessToken") == null)
                {
                    _logger.LogWarning("Token no encontrado para el usuario {Email}", userEmail);
                    return new JsonResult(new { success = false, message = "No se ha podido autenticar con el API. Por favor, vuelve a iniciar sesión." });
                }

 
                if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.CorreoElectronico) || 
                    string.IsNullOrEmpty(request.Telefono) || string.IsNullOrEmpty(request.TipoCliente))
                {
                    return new JsonResult(new { success = false, message = "Los campos obligatorios no pueden estar vacíos" });
                }

 
                var clienteDto = new GestorEventos.Models.ApiModels.ClienteCreateDto
                {
                    TipoCliente = request.TipoCliente,
                    Nombre = request.Nombre,
                    CorreoElectronico = request.CorreoElectronico,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion ?? string.Empty
                };

 
                _logger.LogInformation("Llamando al servicio para crear cliente: {@ClienteDto}", clienteDto);
                var result = await _clienteService.CreateClienteAsync(userEmail, clienteDto);
                
                _logger.LogInformation("Resultado de crear cliente: {Result}", result);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo crear el cliente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
            }
        }

        public class ClienteUpdateRequest
        {
            public string Id { get; set; }
            public string TipoCliente { get; set; }
            public string Nombre { get; set; }
            public string CorreoElectronico { get; set; }
            public string Telefono { get; set; }
            public string Direccion { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateClienteAsync([FromBody] ClienteUpdateRequest request)
        {
            try
            {
 
                if (request == null || string.IsNullOrEmpty(request.Id))
                {
                    _logger.LogWarning("Request body es nulo o no contiene ID");
                    return new JsonResult(new { success = false, message = "No se recibió información válida del cliente" });
                }

 
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo determinar el usuario actual");
                    return new JsonResult(new { success = false, message = "No se pudo determinar el usuario actual" });
                }

 
                if (User.FindFirst("AccessToken") == null)
                {
                    _logger.LogWarning("Token no encontrado para el usuario {Email}", userEmail);
                    return new JsonResult(new { success = false, message = "No se ha podido autenticar con el API. Por favor, vuelve a iniciar sesión." });
                }

 
                if (string.IsNullOrEmpty(request.Nombre) || string.IsNullOrEmpty(request.CorreoElectronico) || 
                    string.IsNullOrEmpty(request.Telefono) || string.IsNullOrEmpty(request.TipoCliente))
                {
                    return new JsonResult(new { success = false, message = "Los campos obligatorios no pueden estar vacíos" });
                }

 
                var clienteDto = new GestorEventos.Models.ApiModels.ClienteCreateDto
                {
                    TipoCliente = request.TipoCliente,
                    Nombre = request.Nombre,
                    CorreoElectronico = request.CorreoElectronico,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion ?? string.Empty
                };

 
                _logger.LogInformation("Llamando al servicio para actualizar cliente {Id}: {@ClienteDto}", request.Id, clienteDto);
                var result = await _clienteService.UpdateClienteAsync(request.Id, clienteDto);
                
                _logger.LogInformation("Resultado de actualizar cliente: {Result}", result);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo actualizar el cliente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
            }
        }

        public class ClienteDeleteRequest
        {
            public string Id { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteClienteAsync([FromBody] ClienteDeleteRequest request)
        {
            try
            {
 
                if (request == null || string.IsNullOrEmpty(request.Id))
                {
                    _logger.LogWarning("Request body es nulo o no contiene ID");
                    return new JsonResult(new { success = false, message = "No se recibió información válida del cliente" });
                }

 
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("No se pudo determinar el usuario actual");
                    return new JsonResult(new { success = false, message = "No se pudo determinar el usuario actual" });
                }

 
                if (User.FindFirst("AccessToken") == null)
                {
                    _logger.LogWarning("Token no encontrado para el usuario {Email}", userEmail);
                    return new JsonResult(new { success = false, message = "No se ha podido autenticar con el API. Por favor, vuelve a iniciar sesión." });
                }

 
                _logger.LogInformation("Llamando al servicio para eliminar cliente {Id}", request.Id);
                var result = await _clienteService.DeleteClienteAsync(request.Id);
                
                _logger.LogInformation("Resultado de eliminar cliente: {Result}", result);
                
                if (result)
                {
                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "No se pudo eliminar el cliente" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente");
                return new JsonResult(new { success = false, message = "Ocurrió un error al procesar la solicitud" });
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