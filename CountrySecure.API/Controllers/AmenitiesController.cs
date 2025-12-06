using CountrySecure.Application.DTOs.Amenity;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public AmenitiesController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        // --- Método Auxiliar para Seguridad (Auditoría) ---
        private Guid GetCurrentUserId()
        {
            // Busca el ClaimTypes.NameIdentifier (sub) o el claim que uses para almacenar el ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty;
        }

        // -------------------------------------------------------------------
        // POST: Creación de Amenity (201 Created)
        // -------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AmenityCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty) return Unauthorized();

                // Asumo que tu servicio tiene un método AmenityCreateAsync que acepta el DTO y el ID de usuario
                var result = await _amenityService.AmenityCreateAsync(createDto, currentUserId);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Captura errores de lógica de negocio (ej. Amenity ya existe)
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Obtener por ID (200 OK / 404 Not Found)
        // -------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                // El servicio GetAmenityByIdAsync ya maneja la lógica de Not Found/IsDeleted
                var result = await _amenityService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                // Captura el error de 'no encontrada o eliminada' lanzado en el servicio
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // PUT: Actualización de Amenity (200 OK / 404 Not Found)
        // -------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AmenityUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty) return Unauthorized(); // Seguridad

                // Asumo que tu servicio tiene un método AmenityUpdateAsync que acepta el DTO, ID y el usuario
                var result = await _amenityService.AmenityUpdateAsync(id, updateDto, currentUserId);

                if (result == null)
                {
                    return NotFound($"Amenity whit Id '{id}' not found.");
                }

                return Ok(result); // 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                // Captura el error si el recurso a actualizar no existe
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // DELETE: Eliminación Lógica (204 No Content / 404 Not Found)
        // -------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _amenityService.DeleteAmenityAsync(id);

                if (!deleted)
                {
                    // Si el servicio devuelve 'false', es probable que no lo haya encontrado
                    return NotFound($"Amenity whit Id '{id}' not found.");
                }

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Consultas de Colección y Filtros (Usando los métodos del servicio)
        // -------------------------------------------------------------------

        // GET /api/amenities?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAllWithTurns([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            // Usamos el método que incluye los turnos, asumiendo que es la consulta base.
            var results = await _amenityService.GetAllAmenitiesWithTurnsAsync(pageNumber, pageSize);
            return Ok(results);
        }

        // GET /api/amenities/name/{amenityName}
        [HttpGet("name/{amenityName}")]
        public async Task<IActionResult> GetByName(string amenityName)
        {
            try
            {
                var result = await _amenityService.GetAmenityByNameAsync(amenityName);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET /api/amenities/capacity?minimumCapacity=10
        [HttpGet("capacity")]
        public async Task<IActionResult> GetByCapacity([FromQuery] int minimumCapacity)
        {
            var results = await _amenityService.GetAmenitiesByCapacityAsync(minimumCapacity);
            return Ok(results);
        }

        // GET /api/amenities/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var results = await _amenityService.GetAmenitiesByStatusAsync(status);
            return Ok(results);
        }
    }
}