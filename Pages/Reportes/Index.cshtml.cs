using GestorEventos.Filters;
using GestorEventos.Models.ApiModels;
using gestor_eventos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace gestor_eventos.Pages.Reportes
{
    [TypeFilter(typeof(AuthorizeFilter))]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ReportesService _reportesService;
        private readonly ExcelExportService _excelExportService;

        public bool LoadError { get; private set; } = false;
        public string ErrorMessage { get; private set; } = string.Empty;
        
        public ResumenEjecutivoResponse? ResumenEjecutivo { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, ReportesService reportesService, ExcelExportService excelExportService)
        {
            _logger = logger;
            _reportesService = reportesService;
            _excelExportService = excelExportService;
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

                // Cargar resumen ejecutivo por defecto (último año)
                var fechaFin = DateTime.Now;
                var fechaInicio = fechaFin.AddYears(-1);
                
                ResumenEjecutivo = await _reportesService.GetResumenEjecutivoAsync(fechaInicio, fechaFin);

                if (ResumenEjecutivo == null)
                {
                    LoadError = true;
                    ErrorMessage = "Error al cargar los datos del resumen ejecutivo.";
                }

                _logger.LogInformation("Cargando vista de reportes para usuario: {Email}", userEmail);
            }
            catch (Exception ex)
            {
                LoadError = true;
                ErrorMessage = "Error al cargar la vista de reportes.";
                _logger.LogError(ex, "Error al cargar reportes para usuario: {Email}", User.FindFirst(ClaimTypes.Email)?.Value);
            }
        }

        public async Task<IActionResult> OnGetResumenEjecutivoAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var resumen = await _reportesService.GetResumenEjecutivoAsync(fechaInicio, fechaFin);
                return new JsonResult(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen ejecutivo");
                return new JsonResult(new { error = "Error al cargar el resumen ejecutivo" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetReportesClientesAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reportes = await _reportesService.GetReportesClientesAsync(fechaInicio, fechaFin);
                return new JsonResult(reportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de clientes");
                return new JsonResult(new { error = "Error al cargar reportes de clientes" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetReportesItemsAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
        {
            try
            {
                var reportes = await _reportesService.GetReportesItemsAsync(fechaInicio, fechaFin, top);
                return new JsonResult(reportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de items");
                return new JsonResult(new { error = "Error al cargar reportes de inventario" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetReportesPagosAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reportes = await _reportesService.GetReportesPagosAsync(fechaInicio, fechaFin);
                return new JsonResult(reportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de pagos");
                return new JsonResult(new { error = "Error al cargar reportes de pagos" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetReportesReservasAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var reportes = await _reportesService.GetReportesReservasAsync(fechaInicio, fechaFin);
                return new JsonResult(reportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de reservas");
                return new JsonResult(new { error = "Error al cargar reportes de reservas" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetReportesServiciosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
        {
            try
            {
                var reportes = await _reportesService.GetReportesServiciosAsync(fechaInicio, fechaFin, top);
                return new JsonResult(reportes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reportes de servicios");
                return new JsonResult(new { error = "Error al cargar reportes de servicios" }) { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> OnGetExportToExcelAsync(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Obtener todos los reportes
                var resumenEjecutivo = await _reportesService.GetResumenEjecutivoAsync(fechaInicio, fechaFin);
                var reportesClientes = await _reportesService.GetReportesClientesAsync(fechaInicio, fechaFin);
                var reportesPagos = await _reportesService.GetReportesPagosAsync(fechaInicio, fechaFin);
                var reportesReservas = await _reportesService.GetReportesReservasAsync(fechaInicio, fechaFin);
                var reportesServicios = await _reportesService.GetReportesServiciosAsync(fechaInicio, fechaFin);
                var reportesItems = await _reportesService.GetReportesItemsAsync(fechaInicio, fechaFin);

                // Generar archivo Excel
                var excelData = _excelExportService.ExportReportesToExcel(
                    resumenEjecutivo,
                    reportesClientes,
                    reportesPagos,
                    reportesReservas,
                    reportesServicios,
                    reportesItems,
                    fechaInicio,
                    fechaFin);

                // Generar nombre del archivo
                var fechaTexto = fechaInicio.HasValue || fechaFin.HasValue
                    ? $"_{fechaInicio?.ToString("yyyyMMdd") ?? "inicio"}_a_{fechaFin?.ToString("yyyyMMdd") ?? "fin"}"
                    : "_completo";
                var fileName = $"Reporte_Gestor_Eventos{fechaTexto}_{DateTime.Now:yyyyMMddHHmm}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar reportes a Excel");
                return BadRequest("Error al generar el archivo Excel");
            }
        }
    }
}