using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.DTOs.Properties;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;

namespace CountrySecure.API.Controllers
{
    // Atributos de la API
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        // --- MÉTODOS DE CONSULTA (GET) ---

        // 1. Método: GET /api/Property?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var propertiesDto = await _propertyService.GetAllPropertiesAsync(pageNumber, pageSize);
            return Ok(propertiesDto); // 200 OK
        }

        // 2. Método: GET /api/Property/available?pageNumber=1&pageSize=10
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var availablePropertiesDto = await _propertyService.GetPropertiesByStatusAsync(
                Domain.Enums.PropertyStatus.Available,
                pageNumber,
                pageSize
            );
            return Ok(availablePropertiesDto); // 200 OK
        }

      


        // 3. Método: GET /api/Property/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var propertyDto = await _propertyService.GetPropertyByIdAsync(id);

            if (propertyDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(propertyDto); // 200 OK
        }

        // 4. Método: GET /api/Property/owner/{ownerId}
        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetByOwner(Guid ownerId)
        {
            var propertiesDto = await _propertyService.GetPropertiesByOwnerId(ownerId);

            if (propertiesDto == null)
            {
                return NotFound();
            }
            return Ok(propertiesDto);
        }

        // 5. Método: GET /api/Property/lot/{lotId}
        [HttpGet("lot/{lotId}")]
        public async Task<IActionResult> GetByLot(Guid lotId)
        {
            var propertiesDto = await _propertyService.GetPropertiesByLotIdAsync(lotId);

            if (propertiesDto == null || !propertiesDto.Any()) // Verifica si la lista está vacía o es nula
            {
                return NotFound();
            }
            return Ok(propertiesDto);
        }

        // --- MÉTODOS DE ESCRITURA ---

        // 6. Método: POST /api/Property (Creación)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreatePropertyDto dto)
        {
            // Valida los Data Annotations del DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            var propertyDto = await _propertyService.AddNewPropertyAsync(dto);

            // 201 Created. propertyDto.IdProperty debe ser el ID generado.
            return CreatedAtAction(nameof(GetById), new { id = propertyDto.PropertyId }, propertyDto);
        }

        // 7. Método: PUT /api/Property/{id} (Actualización SEGURA)
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdatePropertyDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            // VALIDACIÓN CRÍTICA: La ID de la URL debe coincidir con la ID del DTO.
            if (id != updateDto.PropertyId)
            {
                return BadRequest(new { message = "ID mismatch between URL and body." });
            }

            // 1. EXTRAER EL ID DEL USUARIO DESDE EL TOKEN
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            // Verificar si el Claim existe y si es convertible a Guid
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                // Devuelve 401 si no hay token o el ID es inválido (aunque el middleware debería manejarlo primero)
                return Unauthorized();
            }

            try
            {
                // 2. LLAMAR AL SERVICIO con el argumento faltante
                await _propertyService.UpdatePropertyAsync(updateDto, currentUserId);

                return NoContent(); // 204 No Content
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // 404 Not Found si el servicio no encuentra el recurso
            }
            catch (UnauthorizedAccessException)
            {
                // Captura si el servicio verifica que el usuario no es el dueño (403 Forbidden)
                return Forbid();
            }
            
        }

        // 8. Método: DELETE /api/Property/{id} (Baja Lógica SEGURA)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            // 1.  EXTRACCIÓN DEL ID SEGURO DEL TOKEN
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier); // Busca el ID en el token

            //  Si usas Guid, el Claim también debe ser Guid. Asegúrate de que el Claim sea el correcto (ej. "Id")
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                // Si no se encuentra el token o es inválido, devuelve 401
                return Unauthorized();
            }

            try
            {
                // 2. Llama al servicio, pasando el ID del recurso y el ID del usuario logueado.
                bool deleted = await _propertyService.SoftDeletePropertyAsync(id, currentUserId);

                if (!deleted)
                {
                    return NotFound(); // 404 Not Found (La propiedad no existe)
                }
                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException)
            {
                // Captura la excepción que el Servicio lanza si el usuario no es el dueño
                return Forbid(); // 403 Forbidden
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // 404 Not Found
            }
        }
    }
}