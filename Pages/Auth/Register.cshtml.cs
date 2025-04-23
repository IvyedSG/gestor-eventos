using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using GestorEventos.Services;

namespace GestorEventos.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public RegisterModel(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }
        
        // Mensaje de éxito para mostrar en la página de login
        [TempData]
        public string SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El nombre es obligatorio")]
            [Display(Name = "Nombres")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "El apellido es obligatorio")]
            [Display(Name = "Apellidos")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "El correo electrónico es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required(ErrorMessage = "El número de celular es obligatorio")]
            [Phone(ErrorMessage = "El formato del número no es válido")]
            [Display(Name = "Celular")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmPassword { get; set; }
        }

        public class RegisterRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Telefono { get; set; }
        }

        public class RegisterResponse
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string UserId { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
        }

        public IActionResult OnGet()
        {
            // Si el usuario ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try {
                // Crear cliente HTTP
                var client = _httpClientFactory.CreateClient();
                
                // Preparar la solicitud de registro
                var registerRequest = new RegisterRequest
                {
                    Email = Input.Email,
                    Password = Input.Password,
                    Nombre = Input.FirstName,
                    Apellido = Input.LastName,
                    Telefono = Input.PhoneNumber
                };
                
                // Convertir a JSON
                var content = new StringContent(
                    JsonSerializer.Serialize(registerRequest),
                    Encoding.UTF8,
                    "application/json");
                
                // Realizar la petición POST al endpoint de registro
                var response = await client.PostAsync($"{_apiSettings.BaseUrl}/api/auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    // Set flag to show success message
                    TempData["ShowSuccessAlert"] = true;
                    SuccessMessage = "¡Registro exitoso! Por favor, inicia sesión con tus nuevas credenciales.";
                    
                    // Redirect to login page
                    return RedirectToPage("/Auth/Login");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Si el servidor devuelve un error de validación
                    var responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string[]>>(
                            responseContent, 
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        
                        ErrorMessage = errorResponse.First().Value.First();
                    }
                    catch
                    {
                        ErrorMessage = "Ha ocurrido un error al procesar el registro. Por favor, inténtelo de nuevo.";
                    }
                    
                    return Page();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Si el usuario ya existe
                    ErrorMessage = "El correo electrónico ya está registrado.";
                    return Page();
                }
                else
                {
                    // Otros errores
                    ErrorMessage = "Error en el servidor al procesar la solicitud.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Capturar errores de conexión u otros
                ErrorMessage = $"Error al conectar con el servidor: {ex.Message}";
                return Page();
            }
        }
    }
}