using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnController : ControllerBase
    {
        private readonly ITurnService _turnService;

        public TurnController(ITurnService turnService)
        {
            _turnService = turnService;
        }

        // --- Helper para obtener el ID del usuario actual del Token ---
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return currentUserId;
            }
            return null;
        }

        // ----------------------------------------------------------------
        // MÉTODOS DE ESCRITURA (POST, PUT, DELETE)
        // ----------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTurnDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(); // 401 Unauthorized: Token inválido o falta ID
            }

            try
            {
                var turnDto = await _turnService.AddNewTurnAsync(dto, currentUserId.Value);

                // 201 Created
                return CreatedAtAction(nameof(GetById), new { id = turnDto.Id }, turnDto);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Manejo de Violación de Clave Foránea (ej: AmenityId o UserId no existen)
                if (ex.InnerException?.Message?.Contains("23503") == true)
                {
                    return BadRequest(new
                    {
                        message = "Validation Error",
                        detail = "The AmenityId or UserId provided does not exist in the database."
                    });
                }
                // Si es otro error de DB, se trata como un error del servidor.
                return StatusCode(500, "An unexpected database error occurred.");
            }
            catch (Exception)
            {
                // Error genérico no controlado (Ej: error de mapeo, error de lógica)
                return StatusCode(500, "An unexpected error occurred while creating the turn.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateTurnDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(); // 401 Unauthorized
            }

            try
            {
                var updatedTurn = await _turnService.UpdateTurnAsync(id, updateDto, currentUserId.Value);

                if (updatedTurn == null)
                {
                    return NotFound(); // 404 Not Found
                }

                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException ex)
            {
                // Lanzado desde el servicio si el usuario no tiene permisos (403 Forbidden)
                return Forbid(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred during the update.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(); // 401 Unauthorized
            }

            try
            {
                bool deleted = await _turnService.SoftDeleteTurnAsync(id, currentUserId.Value);

                if (!deleted)
                {
                    return NotFound(); // 404 Not Found (Turno no encontrado)
                }

                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred during deletion.");
            }
        }

        // ----------------------------------------------------------------
        // MÉTODOS DE CONSULTA (GET)
        // ----------------------------------------------------------------

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var turnDto = await _turnService.GetTurnByIdAsync(id);
            if (turnDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(turnDto); // 200 OK
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            // Nota: Aquí se permite buscar turnos de cualquier usuario, si se requiere seguridad,
            // se debe añadir lógica para validar si el 'currentUserId' puede ver los turnos de 'userId'.
            var turns = await _turnService.GetTurnsByUserIdAsync(userId);
            return Ok(turns); // 200 OK (devuelve [] si no hay turnos)
        }

        [HttpGet("amenity/{amenityId}")]
        public async Task<IActionResult> GetByAmenityId(Guid amenityId)
        {
            var turns = await _turnService.GetTurnsByAmenityIdAsync(amenityId);
            return Ok(turns);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Validaciones básicas de rango
            if (startDate > endDate)
            {
                return BadRequest("The start date cannot be after the end date.");
            }

            var turns = await _turnService.GetTurnsByDateRangeAsync(startDate, endDate);
            return Ok(turns);
        }
    }
}