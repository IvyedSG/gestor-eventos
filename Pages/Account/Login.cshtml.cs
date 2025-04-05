using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GestorEventos.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // En un entorno real, aquí validarías las credenciales contra tu base de datos
            // Para este ejemplo, usamos credenciales hardcodeadas
            if (Input.Email == "admin@admin.com" && Input.Password == "admin123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Email, Input.Email),
                    new Claim(ClaimTypes.Role, "Administrador")
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

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Correo electrónico o contraseña incorrectos.";
            return Page();
        }
    }
}