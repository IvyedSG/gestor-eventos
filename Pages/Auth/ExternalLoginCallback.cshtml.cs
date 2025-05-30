using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GestorEventos.Pages.Auth 
{
    public class ExternalLoginCallbackModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
                return RedirectToPage("/Auth/Login");
                
            var externalUser = result.Principal;
            if (externalUser == null)
                return RedirectToPage("/Auth/Login");
                
            var userIdClaim = externalUser.FindFirst(ClaimTypes.NameIdentifier);
            var emailClaim = externalUser.FindFirst(ClaimTypes.Email);
            var nameClaim = externalUser.FindFirst(ClaimTypes.Name);
            
            if (userIdClaim == null || emailClaim == null)
                return RedirectToPage("/Auth/Login");
            
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nameClaim?.Value ?? "Usuario de Google"),
                new Claim(ClaimTypes.Email, emailClaim.Value),
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
                
            return RedirectToPage("/Index");
        }
    }
}