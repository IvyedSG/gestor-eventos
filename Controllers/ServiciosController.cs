using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gestor_eventos.Models.ApiModels;
using gestor_eventos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace gestor_eventos.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly ILogger<ServiciosController> _logger;
        private readonly ServicioService _servicioService;

        public ServiciosController(
            ILogger<ServiciosController> logger,
            ServicioService servicioService)
        {
            _logger = logger;
            _servicioService = servicioService;
        }

        [HttpPost("handler=Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] ServicioCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Recibida solicitud para crear servicio: {NombreServicio}", model.NombreServicio);
            
            var result = await _servicioService.CreateServicioAsync(model);
            
            if (result == null)
            {
                _logger.LogWarning("Error al crear servicio: {NombreServicio}", model.NombreServicio);
                return StatusCode(500, new { message = "No se pudo crear el servicio" });
            }

            _logger.LogInformation("Servicio creado exitosamente: {Id}", result.Id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [FromBody] ServicioEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Recibida solicitud para editar servicio: {Id}", id);
            
            var result = await _servicioService.UpdateServicioAsync(id, model);
            
            if (result == null)
            {
                _logger.LogWarning("Error al editar servicio: {Id}", id);
                return StatusCode(500, new { message = "No se pudo editar el servicio" });
            }

            _logger.LogInformation("Servicio editado exitosamente: {Id}", result.Id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID del servicio es requerido");
            }

            _logger.LogInformation("Recibida solicitud para eliminar servicio: {Id}", id);
            
            var result = await _servicioService.DeleteServicioAsync(id);
            
            if (!result)
            {
                _logger.LogWarning("Error al eliminar servicio: {Id}", id);
                return StatusCode(500, new { message = "No se pudo eliminar el servicio" });
            }

            _logger.LogInformation("Servicio eliminado exitosamente: {Id}", id);
            return Ok(new { message = "Servicio eliminado exitosamente" });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var servicios = await _servicioService.GetServiciosAsync();
                return Ok(servicios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener servicios");
                return StatusCode(500, "Error al obtener la lista de servicios");
            }
        }
    }
}