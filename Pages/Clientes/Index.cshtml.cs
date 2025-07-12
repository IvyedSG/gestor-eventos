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
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

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
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string TypeFilter { get; set; } = string.Empty;

        // Propiedades para paginación
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public List<Client> Clients { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasToken { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                
                // Verificar token
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

                // Obtener todos los clientes
                var apiClients = await _clienteService.GetClientesByUsuarioAsync(userEmail);
                
                if (apiClients.Count == 0)
                {
                    _logger.LogInformation("No se encontraron clientes para el usuario {Email}", userEmail);
                }

                // Convertir a modelo local
                var allClients = apiClients.Select(c => new Client
                {
                    Id = c.Id,
                    Name = c.NombreUsuario,
                    Email = c.CorreoUsuario, 
                    Address = c.Direccion,
                    Type = c.TipoCliente == "INDIVIDUAL" ? "Individual" : "Empresa",
                    Ruc = c.Ruc,
                    RazonSocial = c.RazonSocial,
                    Phone = c.Telefono,
                    EventCount = c.TotalReservas,
                    LastReservation = c.UltimaFechaReserva
                }).ToList();

                // Aplicar filtros
                var filteredClients = allClients;

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    filteredClients = filteredClients.Where(c => 
                        c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        c.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (c.Phone != null && c.Phone.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Address != null && c.Address.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(TypeFilter))
                {
                    filteredClients = filteredClients.Where(c => c.Type.Equals(TypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Calcular totales para paginación
                TotalCount = filteredClients.Count;
                TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

                // Asegurar que CurrentPage esté dentro del rango válido
                if (CurrentPage < 1) CurrentPage = 1;
                if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

                // Aplicar paginación
                Clients = filteredClients
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                _logger.LogInformation("Loaded {Count} clients (Page {CurrentPage} of {TotalPages})", 
                    Clients.Count, CurrentPage, TotalPages);
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
            public string TipoCliente { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string CorreoElectronico { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string Ruc { get; set; } = string.Empty;
            public string RazonSocial { get; set; } = string.Empty;
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

                // *** VALIDACIONES MEJORADAS ***
                var validationResult = ValidateClientData(request);
                if (!validationResult.IsValid)
                {
                    return new JsonResult(new { success = false, message = validationResult.ErrorMessage });
                }

                var clienteDto = new GestorEventos.Models.ApiModels.ClienteCreateDto
                {
                    TipoCliente = request.TipoCliente,
                    Nombre = request.Nombre,
                    CorreoElectronico = request.CorreoElectronico,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion ?? string.Empty,
                    Ruc = request.Ruc ?? string.Empty,
                    RazonSocial = request.RazonSocial ?? string.Empty
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
            public string Id { get; set; } = string.Empty;
            public string TipoCliente { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string CorreoElectronico { get; set; } = string.Empty;
            public string Telefono { get; set; } = string.Empty;
            public string Direccion { get; set; } = string.Empty;
            public string Ruc { get; set; } = string.Empty;
            public string RazonSocial { get; set; } = string.Empty;
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

                // *** VALIDACIONES MEJORADAS ***
                var validationResult = ValidateClientData(request);
                if (!validationResult.IsValid)
                {
                    return new JsonResult(new { success = false, message = validationResult.ErrorMessage });
                }

                var clienteDto = new GestorEventos.Models.ApiModels.ClienteCreateDto
                {
                    TipoCliente = request.TipoCliente,
                    Nombre = request.Nombre,
                    CorreoElectronico = request.CorreoElectronico,
                    Telefono = request.Telefono,
                    Direccion = request.Direccion ?? string.Empty,
                    Ruc = request.Ruc ?? string.Empty,
                    RazonSocial = request.RazonSocial ?? string.Empty
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
            public string Id { get; set; } = string.Empty;
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

        private (bool IsValid, string ErrorMessage) ValidateClientData(ClienteCreateRequest request)
        {
            // Validar campos obligatorios
            if (string.IsNullOrEmpty(request.Nombre?.Trim()))
            {
                return (false, "El nombre es obligatorio");
            }

            if (string.IsNullOrEmpty(request.CorreoElectronico?.Trim()))
            {
                return (false, "El correo electrónico es obligatorio");
            }

            if (string.IsNullOrEmpty(request.Telefono?.Trim()))
            {
                return (false, "El teléfono es obligatorio");
            }

            if (string.IsNullOrEmpty(request.TipoCliente))
            {
                return (false, "El tipo de cliente es obligatorio");
            }

            // Validar formato de correo electrónico
            if (!IsValidEmail(request.CorreoElectronico))
            {
                return (false, "El formato del correo electrónico no es válido");
            }

            // Validar formato de teléfono (9 dígitos)
            if (!IsValidPhone(request.Telefono))
            {
                return (false, "El teléfono debe contener exactamente 9 dígitos");
            }

            // Validaciones específicas para empresas
            if (request.TipoCliente == "EMPRESA")
            {
                if (string.IsNullOrEmpty(request.Ruc?.Trim()))
                {
                    return (false, "El RUC es obligatorio para empresas");
                }

                if (string.IsNullOrEmpty(request.RazonSocial?.Trim()))
                {
                    return (false, "La razón social es obligatoria para empresas");
                }

                if (!IsValidRuc(request.Ruc))
                {
                    return (false, "El RUC debe contener exactamente 11 dígitos");
                }
            }

            return (true, string.Empty);
        }

        // *** SOBRECARGA PARA ClienteUpdateRequest ***
        private (bool IsValid, string ErrorMessage) ValidateClientData(ClienteUpdateRequest request)
        {
            // Crear un objeto ClienteCreateRequest para reutilizar la validación
            var createRequest = new ClienteCreateRequest
            {
                TipoCliente = request.TipoCliente,
                Nombre = request.Nombre,
                CorreoElectronico = request.CorreoElectronico,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                Ruc = request.Ruc,
                RazonSocial = request.RazonSocial
            };

            return ValidateClientData(createRequest);
        }

        // *** FUNCIONES DE VALIDACIÓN ***
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Solo números, exactamente 9 dígitos
            var phoneRegex = new Regex(@"^[0-9]{9}$");
            return phoneRegex.IsMatch(phone);
        }

        private bool IsValidRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                return false;

            // Solo números, exactamente 11 dígitos
            var rucRegex = new Regex(@"^[0-9]{11}$");
            return rucRegex.IsMatch(ruc);
        }
    }

    public class Client
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty; // Agregamos esta propiedad
        public int EventCount { get; set; }
        public DateTime? LastReservation { get; set; }
    }
}