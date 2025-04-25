using GestorEventos.Filters;
using GestorEventos.Models.ApiModels;
using gestor_eventos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace gestor_eventos.Pages.Dashboard
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DashboardService _dashboardService;

        public DashboardResponse DashboardData { get; private set; }
        public bool LoadError { get; private set; } = false;
        public string ErrorMessage { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, DashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userEmail))
                {
                    LoadError = true;
                    ErrorMessage = "No se pudo obtener el correo electrónico del usuario.";
                    return;
                }

                DashboardData = await _dashboardService.GetDashboardDataAsync(userEmail);

                if (DashboardData == null)
                {
                    LoadError = true;
                    ErrorMessage = "Error al cargar los datos del dashboard.";
                }
            }
            catch (Exception ex)
            {
                LoadError = true;
                ErrorMessage = "Ocurrió un error al cargar los datos del dashboard.";
                _logger.LogError(ex, "Error al cargar el dashboard");
            }
        }
    }
}