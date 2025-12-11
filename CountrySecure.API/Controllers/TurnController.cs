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

        // Dentro de TurnController.cs

        [HttpPut("{id:guid}")] // Aseguramos la restricción de GUID en la ruta
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateTurnDto updateDto)
        {
            // 1. Validar Data Annotations del DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            // Extraemos el ID del usuario (asumiendo que GetCurrentUserId() devuelve Guid?)
            var currentUserId = GetCurrentUserId();

            // 2. Validación de Autenticación (Si GetCurrentUserId devuelve Guid? y es null)
            if (!currentUserId.HasValue)
            {
                // Esto captura la falta de token o un token inválido/vacío
                return Unauthorized(); // 401 Unauthorized
            }

            try
            {
                // 3. Llamada al servicio con la carga ansiosa para el retorno (200 OK)
                var updatedTurnDto = await _turnService.UpdateTurnAsync(id, updateDto, currentUserId.Value);

                // 4. Manejo de Recurso No Encontrado (Si el servicio devuelve null)
                if (updatedTurnDto == null)
                {
                    return NotFound($"Turno con ID {id} no encontrado."); // 404 Not Found
                }

                // 5. Retorno de Éxito (200 OK con el cuerpo del objeto actualizado)
                return Ok(updatedTurnDto);
            }
            catch (UnauthorizedAccessException) // Lógica de negocio del servicio (403 Forbidden)
            {
                // Capturado si el servicio determina que el usuario (currentUserId) no puede modificar este turno.
                return Forbid();
            }
            catch (KeyNotFoundException ex) // Lógica de negocio (404 Not Found)
            {
                // Capturado si el ID del turno o una FK (AmenityId, UserId) no existe.
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                // Manejo de error genérico (ej. problemas de base de datos)
                return StatusCode(500, "An unexpected error occurred during the update.");
            }
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            // 2. Extraemos el ID del usuario
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                // 3. Llamada al servicio que ahora devuelve el DTO o null
                var updatedTurnDto = await _turnService.SoftDeleteTurnAsync(id, currentUserId.Value);

                if (updatedTurnDto == null)
                {
                    return NotFound($"Turno con ID {id} no encontrado."); // 404 Not Found
                }

                // 4. Retorno de éxito (200 OK y el DTO actualizado)
                var action = updatedTurnDto.Status == "Active" ? "reactivado" : "desactivado";

                return Ok(new
                {
                    Message = $"El Turno con ID {id} ha sido {action} exitosamente.",
                    Turn = updatedTurnDto // Devuelve el DTO completo y actualizado
                });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado durante el cambio de estado.");
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