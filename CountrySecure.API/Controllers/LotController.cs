using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.DTOs.Lots;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using CountrySecure.Domain.Enums;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class LotController : ControllerBase
    {
        private readonly ILotService _lotService;

        public LotController(ILotService lotService)
        {
            _lotService = lotService;
        }

        // --- MÉTODOS DE CONSULTA (GET) ---

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var lotsDto = await _lotService.GetAllLotsAsync(pageNumber, pageSize);
            return Ok(lotsDto);
        }

        [HttpGet("block-names")]
        public async Task<IActionResult> GetAllBlockNames()
        {
            var blockNames = await _lotService.GetAllBlockNamesAsync();
            return Ok(blockNames);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lotDto = await _lotService.GetLotByIdAsync(id);
            if (lotDto == null)
            {
                return NotFound();
            }
            return Ok(lotDto);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetLotsByStatus([FromQuery] LotStatus status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var lotsDto = await _lotService.GetLotsByStatusAsync(status, pageNumber, pageSize);

          
            return Ok(lotsDto);
        }

        // --- MÉTODOS DE ESCRITURA ---

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateLotDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Extracción del ID del usuario creador (del token)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized();
            }

            // 2. Llamada al servicio con el ID del creador
            var lotDto = await _lotService.AddNewLotAsync(dto, currentUserId);

            return CreatedAtAction(nameof(GetById), new { id = lotDto.LotId }, lotDto);
        }

        [HttpPut("{id:guid}")] // Añadir restricción :guid
        public async Task<IActionResult> put(Guid id, [FromBody] UpdateLotDto updatedto)
        {
            // 1. Extracción y validación del ID del usuario del token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
               
                var updatedLotDto = await _lotService.UpdateAsync(updatedto, id, currentUserId);

               
                return Ok(updatedLotDto);
            }
            catch (KeyNotFoundException ex)
            {
                // El servicio ahora lanza la excepción si no encuentra el Lote.
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error interno al actualizar el lote.");
            }
        }
        [HttpPatch("{id:guid}/SoftDelete")] 
        public async Task<IActionResult> SoftDeleteToggle(Guid id)
        {
            
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized();
            }

            try
            {
                
                var updatedLotDto = await _lotService.SoftDeleteToggleAsync(id, currentUserId);

                if (updatedLotDto == null)
                {
                   
                    return NotFound(new { message = $"Lot with ID {id} not found." });
                }

              
                var action = updatedLotDto.Status == "Active" ? "reactivated" : "deactivated"; 

                return Ok(new
                {
                    Message = $"The Lot with ID {id} has been {action} successfully.",
                    Lot = updatedLotDto 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred while updating the lot status.", detail = ex.Message });
            }
        }



    }
}