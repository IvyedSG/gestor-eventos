using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GestorEventos.Pages.Account
{
    public class ExternalLoginCallbackModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
                return RedirectToPage("/Account/Login");
                
            var externalUser = result.Principal;
            if (externalUser == null)
                return RedirectToPage("/Account/Login");
                
            // Extraer la información del usuario desde Google
            var userIdClaim = externalUser.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = externalUser.FindFirst(ClaimTypes.Email);
            var nameClaim = externalUser.FindFirst(ClaimTypes.Name);
            
            if (userIdClaim == null || emailClaim == null)
                return RedirectToPage("/Account/Login");
            
            // Aquí deberías validar si el usuario existe en tu base de datos
            // o crearlo si es la primera vez que inicia sesión
            // Para este ejemplo, simplemente creamos un nuevo ClaimsPrincipal
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nameClaim?.Value ?? "Usuario de Google"),
                new Claim(ClaimTypes.Email, emailClaim.Value),
                new Claim(ClaimTypes.Role, "Usuario") // Asignar un rol predeterminado
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
                
            return RedirectToPage("/Index");
        }
    }
}