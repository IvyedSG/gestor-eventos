using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestorEventos.Pages.Auth
{
    public class ExternalLoginModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Provider { get; set; } = "Google";

        public string ProviderDisplayName => Provider;

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; } = "/Index";

        public IActionResult OnGet()
        {
            // Redirigir al usuario al proveedor de autenticación externo
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = Url.Page("/Auth/ExternalLoginCallback"),
                Items = 
                {
                    { "returnUrl", ReturnUrl },
                    { "scheme", Provider }
                }
            };
            
            return Challenge(properties, Provider);
        }
    }
}