using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gestor_eventos.Pages.Servicios
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public List<ServiceModel> Services { get; set; }

        public void OnGet()
        {
            // En un caso real, cargarías datos desde tu base de datos
            // Esta es una implementación simulada para demostración
            Services = GetSampleServices();

            // Aplicar filtros si existen
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Services = Services.Where(s => 
                    s.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    s.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                Services = Services.Where(s => 
                    s.Category.Equals(CategoryFilter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
        }

        private List<ServiceModel> GetSampleServices()
        {
            // Datos de muestra para demostración
            return new List<ServiceModel>
            {
                new ServiceModel
                {
                    Id = 1,
                    Name = "Catering Premium",
                    Description = "Servicio de catering premium para eventos exclusivos. Incluye menú personalizado, decoración de mesa y personal de servicio.",
                    Category = "Catering",
                    BasePrice = 5500.00m,
                    EventTypes = "Boda, Corporativo, Graduación",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                },
                new ServiceModel
                {
                    Id = 2,
                    Name = "Decoración Elegante",
                    Description = "Servicio completo de decoración temática con flores naturales, telas y elementos decorativos premium. Personalizable según el tipo de evento.",
                    Category = "Decoración",
                    BasePrice = 3200.00m,
                    EventTypes = "Boda, Aniversario, Graduación",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                },
                new ServiceModel
                {
                    Id = 3,
                    Name = "DJ y Sonido Profesional",
                    Description = "Equipo de audio profesional con DJ experimentado, luces LED y efectos especiales para mantener el ambiente festivo durante todo el evento.",
                    Category = "Entretenimiento",
                    BasePrice = 2800.00m,
                    EventTypes = "Boda, Cumpleaños, Corporativo",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                },
                new ServiceModel
                {
                    Id = 4,
                    Name = "Proyección y Pantallas",
                    Description = "Equipos de proyección en alta definición con pantallas de diversos tamaños. Incluye técnico especializado durante todo el evento.",
                    Category = "Audio y Video",
                    BasePrice = 1500.00m,
                    EventTypes = "Corporativo, Graduación, Conferencia",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                },
                new ServiceModel
                {
                    Id = 5,
                    Name = "Mesas y Sillas VIP",
                    Description = "Mobiliario premium para eventos elegantes. Incluye mesas redondas, sillas Tiffany, manteles de alta calidad y montaje completo.",
                    Category = "Mobiliario",
                    BasePrice = 3800.00m,
                    EventTypes = "Boda, Gala, Aniversario",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                },
                new ServiceModel
                {
                    Id = 6,
                    Name = "Fotografía y Video",
                    Description = "Cobertura completa de fotografía y video para tu evento. Incluye equipo profesional, edición básica y entrega de material en alta resolución.",
                    Category = "Audio y Video",
                    BasePrice = 4200.00m,
                    EventTypes = "Boda, Cumpleaños, Bautizo",
                    MainImage = "/assets/img/placeholder-img.png",
                    Images = new List<string>
                    {
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png",
                        "/assets/img/placeholder-img.png"
                    }
                }
            };
        }
    }

    public class ServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal BasePrice { get; set; }
        public string EventTypes { get; set; }
        public string MainImage { get; set; }
        public List<string> Images { get; set; }
    }
}