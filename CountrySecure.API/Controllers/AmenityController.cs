using CountrySecure.Application.DTOs.Amenities;
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
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public AmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        // --- Método Auxiliar para Seguridad (Auditoría) ---
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return null;
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

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _amenityService.AmenityCreateAsync(createDto, currentUserId.Value);

                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // Captura errores de lógica de negocio (ej. Amenity ya existe)
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Manejo genérico de errores (ej. errores de DB no controlados)
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
        // PUT: Actualización de Amenity (204 No Content / 404 Not Found)
        // -------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AmenityUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _amenityService.AmenityUpdateAsync(id, updateDto, currentUserId.Value);

                if (result == null)
                {
                    return NotFound($"Amenity with Id '{id}' not found.");
                }

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


        // -------------------------------------------------------------------
        // PATCH: Toggle (Activación/Desactivación Lógica) (200 OK / 404 Not Found)
        // -------------------------------------------------------------------
        [HttpPatch("{id:guid}/SoftDelete")] // Endpoint estandarizado
        public async Task<IActionResult> SoftDeleteToggle(Guid id) // Nombre estandarizado
        {
            // 1. Extraer ID del usuario
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(); // 401 Unauthorized
            }

            try
            {
                // 2. Llama al servicio estandarizado
                var updatedAmenityDto = await _amenityService.SoftDeleteToggleAsync(id, currentUserId.Value);

                if (updatedAmenityDto == null)
                {
                    return NotFound($"Amenity con Id '{id}' no encontrado."); // 404 Not Found
                }

                // 3. Retorno de éxito (200 OK y el DTO actualizado)
                // Usamos la propiedad Status de la BaseEntity (string)
                var action = updatedAmenityDto.Status == "Active" ? "reactivado" : "desactivado";

                return Ok(new
                {
                    Message = $"La Amenity con ID {id} ha sido {action} exitosamente.",
                    Amenity = updatedAmenityDto // Devuelve el DTO completo y actualizado
                });

            }
            catch (Exception ex)
            {
                // Se deja manejo genérico, asumiendo que el servicio ya maneja Not Found (devolviendo null)
                // y que no lanza UnauthorizedAccessException.
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Consultas de Colección y Filtros
        // -------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> GetAllWithTurns([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            // Nota: Aquí se usa el método GetAllAmenitiesWithTurnsAsync, el nombre del método es confuso,
            // pero el método en el servicio está definido.
            var results = await _amenityService.GetAllAmenitiesWithTurnsAsync(pageNumber, pageSize);
            return Ok(results);
        }

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

        [HttpGet("capacity")]
        public async Task<IActionResult> GetByCapacity([FromQuery] int minimumCapacity)
        {
            var results = await _amenityService.GetAmenitiesByCapacityAsync(minimumCapacity);
            return Ok(results);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var results = await _amenityService.GetAmenitiesByStatusAsync(status);
            return Ok(results);
        }
    }
}