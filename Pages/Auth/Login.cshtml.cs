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
        

        [TempData]
        public bool ShowSuccessAlert { get; set; }
        

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

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            if (ShowSuccessAlert && !string.IsNullOrEmpty(RedirectPath))
            {
               
                var redirectPath = RedirectPath;
                
                RedirectPath = null;

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
              
                var client = _httpClientFactory.CreateClient();
                var apiUrl = $"{_configuration["ApiSettings:BaseUrl"]}/api/auth/login";

               
                var loginData = new
                {
                    Email = Input.Email,
                    Password = Input.Password
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json");

              
                var response = await client.PostAsync(apiUrl, content);

              
                if (response.IsSuccessStatusCode)
                {
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, options);

                   
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{authResponse.Nombre} {authResponse.Apellido}"),
                        new Claim(ClaimTypes.Email, authResponse.Email),
                        new Claim("UserId", authResponse.UserId),
                       
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

       
                    SuccessMessage = $"¡Bienvenid@ {authResponse.Nombre}!";
                    ShowSuccessAlert = true;
                    
         
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