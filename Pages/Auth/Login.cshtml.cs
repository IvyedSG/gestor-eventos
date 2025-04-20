using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

using System.Text;
using System.Text.Json;


namespace GestorEventos.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }
        
        [TempData]
        public string SuccessMessage { get; set; }
        
        // Flag para señalar que necesitamos mostrar el alert de éxito
        [TempData]
        public bool ShowSuccessAlert { get; set; }
        
        // Ruta de redirección después del login exitoso
        [TempData]
        public string RedirectPath { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public IActionResult OnGet()
        {
            // Si el usuario ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            // Si venimos de un login exitoso con el flag ShowSuccessAlert,
            // permitir mostrar la página con el flag activo para que el alert se muestre
            if (ShowSuccessAlert && !string.IsNullOrEmpty(RedirectPath))
            {
                // Preparamos la ruta de redirección
                var redirectPath = RedirectPath;
                
                // Limpiamos las variables TempData para la próxima visita
                RedirectPath = null;
                
                // Mantenemos ShowSuccessAlert para que la vista pueda leerlo
                // Se limpiará automáticamente después de que la vista lo lea
                
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor, completa todos los campos correctamente.";
                return Page();
            }

            try 
            {
                // Crear el cliente HTTP
                var client = _httpClientFactory.CreateClient();
                var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/api/auth/login";

                // Crear el contenido de la solicitud
                var loginData = new
                {
                    Email = Input.Email,
                    Password = Input.Password
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json");

                // Enviar la solicitud al API
                var response = await client.PostAsync(apiUrl, content);

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer y deserializar la respuesta
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, options);

                    // Crear los claims para la autenticación
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{authResponse.Nombre} {authResponse.Apellido}"),
                        new Claim(ClaimTypes.Email, authResponse.Email),
                        new Claim("UserId", authResponse.UserId),
                        // Almacenar el token en las claims
                        new Claim("AccessToken", authResponse.Token)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddDays(7)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        authProperties);

                    // Configurar el mensaje de éxito y activar el flag para mostrar alert
                    SuccessMessage = $"¡Bienvenid@ {authResponse.Nombre}!";
                    ShowSuccessAlert = true;
                    
                    // En lugar de redireccionar inmediatamente, mostramos primero el mensaje de éxito
                    return Page();
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    ErrorMessage = "Error de autenticación. Por favor, verifica tus credenciales.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
                return Page();
            }
        }

        private class AuthResponse
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string UserId { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
        }
    }
}