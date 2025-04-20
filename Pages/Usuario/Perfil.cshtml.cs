using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace gestor_eventos.Pages.Usuario
{
    public class PerfilModel : PageModel
    {
        [BindProperty]
        public PerfilInputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public PerfilModel()
        {
            Input = new PerfilInputModel();
        }

        public void OnGet()
        {
            // Aquí cargaríamos los datos del usuario actual
            // Por ejemplo:
            Input.Nombre = User.Claims.FirstOrDefault(c => c.Type == "Nombre")?.Value;
            Input.Apellido = User.Claims.FirstOrDefault(c => c.Type == "Apellido")?.Value;
            Input.Telefono = User.Claims.FirstOrDefault(c => c.Type == "Telefono")?.Value;
            Input.Email = User.Identity.Name;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Aquí iría el código para actualizar los datos del usuario en la base de datos
            
            StatusMessage = "¡Tu perfil ha sido actualizado correctamente!";
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
    }
}