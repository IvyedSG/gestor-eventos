using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GestorEventos.Models.ApiModels;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace gestor_eventos.Pages.Usuario
{
    public class PerfilModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PerfilModel> _logger;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public PerfilInputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public PerfilModel(HttpClient httpClient, ILogger<PerfilModel> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            Input = new PerfilInputModel();
        }

        public async Task OnGetAsync()
        {
            try
            {
 
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                
 
                var token = User.FindFirst("AccessToken")?.Value;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("Token o email de usuario no disponible");
 
                    Input.Nombre = User.Claims.FirstOrDefault(c => c.Type == "Nombre")?.Value;
                    Input.Apellido = User.Claims.FirstOrDefault(c => c.Type == "Apellido")?.Value;
                    Input.Telefono = User.Claims.FirstOrDefault(c => c.Type == "Telefono")?.Value;
                    Input.Email = userEmail ?? User.Identity.Name;
                    return;
                }

                _logger.LogInformation("Obteniendo datos del usuario con email: {Email}", userEmail);

 
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

 
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/usuarios/{userEmail}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Respuesta del API: {Response}", content);
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var usuario = JsonSerializer.Deserialize<UsuarioResponse>(content, options);
                    
 
                    Input.Nombre = usuario.Nombre;
                    Input.Apellido = usuario.Apellido;
                    Input.Telefono = usuario.Telefono;
                    Input.Email = usuario.Correo;
                }
                else
                {
                    _logger.LogWarning("Error al obtener datos del usuario. Código: {StatusCode}, URL: {Url}", 
                        (int)response.StatusCode, 
                        $"{apiBaseUrl}/api/usuarios/{userEmail}");
                        
 
                    Input.Nombre = User.Claims.FirstOrDefault(c => c.Type == "Nombre")?.Value;
                    Input.Apellido = User.Claims.FirstOrDefault(c => c.Type == "Apellido")?.Value;
                    Input.Telefono = User.Claims.FirstOrDefault(c => c.Type == "Telefono")?.Value;
                    Input.Email = userEmail;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos del usuario");
 
                Input.Nombre = User.Claims.FirstOrDefault(c => c.Type == "Nombre")?.Value;
                Input.Apellido = User.Claims.FirstOrDefault(c => c.Type == "Apellido")?.Value;
                Input.Telefono = User.Claims.FirstOrDefault(c => c.Type == "Telefono")?.Value;
                Input.Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? User.Identity.Name;
            }
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            _logger.LogInformation("OnPostUpdateAsync iniciado");
            _logger.LogDebug("Datos recibidos: Nombre={Nombre}, Apellido={Apellido}, Teléfono={Telefono}, Email={Email}", 
                Input.Nombre, Input.Apellido, Input.Telefono, Input.Email);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido: {Errors}", string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)));
                return Page();
            }

            try
            {
 
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                        
 
                var token = User.FindFirst("AccessToken")?.Value;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("Token o email de usuario no disponible para actualización");
                    StatusMessage = "No se pudo actualizar el perfil. Inicia sesión nuevamente.";
                    return RedirectToPage();
                }

                _logger.LogInformation("Actualizando perfil para usuario con email: {Email}", userEmail);

 
                var updateData = new
                {
                    nombre = Input.Nombre,
                    apellido = Input.Apellido,
                    telefono = Input.Telefono,
                    verificado = true  // Campo requerido por la API
                };

 
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

 
                var jsonContent = JsonSerializer.Serialize(updateData);
                _logger.LogDebug("Datos JSON a enviar: {JsonContent}", jsonContent);
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

 
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var encodedEmail = Uri.EscapeDataString(userEmail);
                var requestUrl = $"{apiBaseUrl}/api/usuarios/{encodedEmail}";
                
                _logger.LogDebug("Enviando solicitud PUT a: {Url}", requestUrl);
                
 
                var response = await _httpClient.PutAsync(requestUrl, content);
                
 
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Respuesta recibida: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Perfil actualizado correctamente para el usuario: {Email}", userEmail);
                    StatusMessage = "¡Tu perfil ha sido actualizado correctamente!";
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var usuario = JsonSerializer.Deserialize<UsuarioResponse>(responseContent, options);
                    
 
                    Input.Nombre = usuario.Nombre;
                    Input.Apellido = usuario.Apellido;
                    Input.Telefono = usuario.Telefono;
                    
 
                    await UpdateUserClaimsAsync(usuario);
                }
                else
                {
                    _logger.LogWarning("Error al actualizar el perfil. Código: {StatusCode}, Respuesta: {Response}", 
                        (int)response.StatusCode, 
                        responseContent);
                    StatusMessage = "No se pudo actualizar el perfil. Por favor, inténtalo de nuevo.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario");
                StatusMessage = "Ocurrió un error al actualizar el perfil. Por favor, inténtalo más tarde.";
            }

            return RedirectToPage();
        }

        public class PerfilInputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            [Display(Name = "Nombre")]
            [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 2)]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "El apellido es obligatorio")]
            [Display(Name = "Apellido")]
            [StringLength(100, ErrorMessage = "El {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 2)]
            public string Apellido { get; set; }

            [Phone]
            [Display(Name = "Número de teléfono")]
            [Required(ErrorMessage = "El número de teléfono es obligatorio")]
            public string Telefono { get; set; }
            
            [EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }
        }

 
        private class UsuarioResponse
        {
            public string Id { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Correo { get; set; }
            public string Telefono { get; set; }
            public DateTime FechaRegistro { get; set; }
        }

 
        private async Task UpdateUserClaimsAsync(UsuarioResponse usuario)
        {
            try
            {
 
                var identity = (ClaimsIdentity)User.Identity;
                
 
                UpdateOrAddClaim(identity, "Nombre", usuario.Nombre);
                UpdateOrAddClaim(identity, "Apellido", usuario.Apellido);
                UpdateOrAddClaim(identity, "Telefono", usuario.Telefono);
                
 
                var authenticationManager = HttpContext.RequestServices
                    .GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
                    
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true });
                    
                _logger.LogInformation("Claims del usuario actualizados correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar los claims del usuario");
            }
        }

 
        private void UpdateOrAddClaim(ClaimsIdentity identity, string claimType, string claimValue)
        {
            var claim = identity.FindFirst(claimType);
            if (claim != null)
            {
                identity.RemoveClaim(claim);
            }
            identity.AddClaim(new Claim(claimType, claimValue));
        }
    }
}