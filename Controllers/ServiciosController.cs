using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Services;

namespace gestor_eventos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly ServicioService _servicioService;
        private readonly ILogger<ServiciosController> _logger;

        public ServiciosController(ServicioService servicioService, ILogger<ServiciosController> logger)
        {
            _servicioService = servicioService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateServicio([FromQuery] string correo, [FromBody] ServicioCreateModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(correo))
                {
                    return BadRequest("El correo es requerido");
                }

                var result = await _servicioService.CreateServicioAsync(correo, model);
                if (result)
                {
                    return Ok(new { success = true, message = "Servicio creado exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al crear el servicio" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear servicio: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateServicio([FromQuery] string correo, [FromBody] ServicioUpdateModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(correo))
                {
                    return BadRequest("El correo es requerido");
                }

                var result = await _servicioService.UpdateServicioAsync(correo, model);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Servicio actualizado exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al actualizar el servicio" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPut("{correo}/{id}")]
        public async Task<IActionResult> UpdateServicioDetailed(string correo, string id, [FromBody] ServicioUpdateRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(correo))
                {
                    return BadRequest(new { success = false, message = "El correo es requerido" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { success = false, message = "El ID del servicio es requerido" });
                }

                var result = await _servicioService.UpdateServicioDetailedAsync(correo, id, model);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Servicio actualizado exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al actualizar el servicio" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar servicio detallado: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpDelete("{correo}/{id}")]
        public async Task<IActionResult> DeleteServicio(string correo, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(correo))
                {
                    return BadRequest(new { success = false, message = "El correo es requerido" });
                }

                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(new { success = false, message = "El ID del servicio es requerido" });
                }

                var result = await _servicioService.DeleteServicioAsync(correo, id);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Servicio eliminado exitosamente" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Error al eliminar el servicio" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio: {Message}", ex.Message);
                return StatusCode(500, new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}