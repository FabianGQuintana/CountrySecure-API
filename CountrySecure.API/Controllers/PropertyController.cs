using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.DTOs.Properties;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        // --- MÉTODOS DE CONSULTA (GET) --- (NO NECESITAN TRY/CATCH SI SOLO DE VUELVEN LISTAS)

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var propertiesDto = await _propertyService.GetAllPropertiesAsync(pageNumber, pageSize);
            return Ok(propertiesDto);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var availablePropertiesDto = await _propertyService.GetPropertiesByStatusAsync(
                Domain.Enums.PropertyStatus.Available,
                pageNumber,
                pageSize
            );
            return Ok(availablePropertiesDto);
        }

        [HttpGet("{id:guid}")] // Añadir restricción :guid
        public async Task<IActionResult> GetById(Guid id)
        {
            var propertyDto = await _propertyService.GetPropertyByIdAsync(id);

            if (propertyDto == null)
            {
                return NotFound();
            }
            return Ok(propertyDto);
        }

        [HttpGet("owner/{ownerId:guid}")] // Añadir restricción :guid
        public async Task<IActionResult> GetByOwner(Guid ownerId)
        {
            var propertiesDto = await _propertyService.GetPropertiesByOwnerId(ownerId);
            return Ok(propertiesDto);
        }

        [HttpGet("lot/{lotId:guid}")] // Añadir restricción :guid
        public async Task<IActionResult> GetByLot(Guid lotId)
        {
            var propertiesDto = await _propertyService.GetPropertiesByLotIdAsync(lotId);
            return Ok(propertiesDto);
        }

        // --- MÉTODOS DE ESCRITURA (CON MANEJO DE ERRORES) ---

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreatePropertyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. EXTRAER ID Y AUTORIZAR
                var currentUserId = GetCurrentUserId();

                // 2. CREACIÓN (Lanza KeyNotFoundException si LotId es inválido)
                var propertyDto = await _propertyService.AddNewPropertyAsync(dto, currentUserId);

                // 3. 201 Created
                return CreatedAtAction(nameof(GetById), new { id = propertyDto.PropertyId }, propertyDto);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(); // Token inválido o no encontrado
            }
            catch (KeyNotFoundException)
            {
                // Lanza si LotId no existe o está eliminado (lógica del servicio)
                return BadRequest(new { message = "El LotId proporcionado no existe o no es válido para la asignación." });
            }
            catch (Exception ex)
            {
                // Captura errores de DB, de unicidad, etc.
                return StatusCode(500, new
                {
                    message = "Error inesperado al crear la propiedad.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdatePropertyDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. EXTRAER ID Y AUTORIZAR
                var currentUserId = GetCurrentUserId();

                // 2. ACTUALIZACIÓN (Lanza KeyNotFoundException o UnauthorizedAccessException)
                var updatedProperty = await _propertyService.UpdatePropertyAsync(id, updateDto, currentUserId);

                if (updatedProperty == null)
                {
                    return NotFound($"Propiedad con ID {id} no encontrada.");
                }

                return Ok(updatedProperty);
            }
            catch (KeyNotFoundException)
            {
                // Captura si la Propiedad o el nuevo LotId (si se actualizó) no existen
                return NotFound("El recurso principal o la clave foránea (Lot/Usuario) no existen.");
            }
            catch (UnauthorizedAccessException)
            {
                // Captura si el usuario no es el creador
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error inesperado al actualizar la propiedad.", detail = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/soft-delete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var property = await _propertyService.SoftDeleteAsync(id);

            if (property == null)
                return NotFound(new { message = $"Property with id {id} not found" });

            return Ok(property);
        }


        // --- MÉTODO DE UTILIDAD ---

        // Método consolidado para extraer y validar el ID del token
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            if (!Guid.TryParse(userId, out Guid currentUserId))
                throw new UnauthorizedAccessException("User ID in token is not a valid GUID.");

            return currentUserId;
        }
    }
}