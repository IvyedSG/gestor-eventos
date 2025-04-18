using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GestorEventos.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

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

            // Aquí implementarías el registro real del usuario en tu base de datos
            // Este es solo un ejemplo para mostrar el flujo
            
            // Comprobar si el correo ya está en uso (simulated check)
            if (Input.Email == "admin@admin.com") // Simulamos que este correo ya existe
            {
                ErrorMessage = "El correo electrónico ya está registrado.";
                return Page();
            }

            // En una implementación real, aquí crearías el usuario en la base de datos
            // y probablemente enviarías un correo de verificación
            
            // Para este ejemplo, registramos al usuario y lo iniciamos sesión directamente
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"{Input.FirstName} {Input.LastName}"),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.MobilePhone, Input.PhoneNumber),
                new Claim(ClaimTypes.Role, "Usuario")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // Redireccionar al usuario a la página principal
            return RedirectToPage("/Index");
        }
    }
}