using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        // MÉTODOS DE ESCRITURA (POST, PUT, PATCH)
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
                return Unauthorized(); // 401 Unauthorized: Invalid token or missing ID
            }

            try
            {
                var turnDto = await _turnService.AddNewTurnAsync(dto, currentUserId.Value);

                // 201 Created
                return CreatedAtAction(nameof(GetById), new { id = turnDto.Id }, turnDto);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                // Manejo genérico de errores de DB (Ej: Violación de FK)
                return BadRequest(new
                {
                    message = "Validation Error",
                    detail = "The provided AmenityId or UserId does not exist, or the time slot is invalid."
                });
            }
            catch (Exception)
            {
                // Error genérico no controlado
                return StatusCode(500, "An unexpected error occurred while creating the turn.");
            }
        }

        [HttpPut("{id:guid}")] // Aseguramos la restricción de GUID en la ruta
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
                var updatedTurnDto = await _turnService.UpdateTurnAsync(id, updateDto, currentUserId.Value);

                if (updatedTurnDto == null)
                {
                    
                    return NotFound($"Turn with ID {id} not found."); // 404 Not Found
                }

                return Ok(updatedTurnDto);
            }
            catch (UnauthorizedAccessException) // Lógica de negocio del servicio (403 Forbidden)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex) // Lógica de negocio (404 Not Found)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
            
                return StatusCode(500, "An unexpected error occurred during the update.");
            }
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                var updatedTurnDto = await _turnService.SoftDeleteTurnAsync(id, currentUserId.Value);

                if (updatedTurnDto == null)
                {
                    
                    return NotFound($"Turn with ID {id} not found."); // 404 Not Found
                }

                
                var action = updatedTurnDto.Status == "Active" ? "reactivated" : "deactivated";

                return Ok(new
                {
                    Message = $"The Turn with ID {id} has been {action} successfully.",
                    Turn = updatedTurnDto // Devuelve el DTO completo y actualizado
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // 403 Forbidden
            }
            catch (Exception)
            {
               
                return StatusCode(500, "An unexpected error occurred during the status change.");
            }
        }

        // ----------------------------------------------------------------
        // MÉTODOS DE CONSULTA (GET)
        // ----------------------------------------------------------------

        
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var turns = await _turnService.GetAllTurnsAsync(pageNumber, pageSize);
            return Ok(turns);
        }

        [HttpGet("{id:guid}")] // Añadida restricción de GUID
        public async Task<IActionResult> GetById(Guid id)
        {
            var turnDto = await _turnService.GetTurnByIdAsync(id);
            if (turnDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(turnDto); // 200 OK
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var turns = await _turnService.GetTurnsByUserIdAsync(userId);
            return Ok(turns);
        }

        [HttpGet("amenity/{amenityId:guid}")]
        public async Task<IActionResult> GetByAmenityId(Guid amenityId)
        {
            var turns = await _turnService.GetTurnsByAmenityIdAsync(amenityId);
            return Ok(turns);
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
              
                return BadRequest("The start date cannot be after the end date.");
            }

            var turns = await _turnService.GetTurnsByDateRangeAsync(startDate, endDate);
            return Ok(turns);
        }
    }
}