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
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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
        public async Task<IActionResult> GetLotsByStatus([FromQuery] LotStatus status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var lotsDto = await _lotService.GetLotsByStatusAsync(status, pageNumber, pageSize);
            // La verificación de null es innecesaria si el servicio devuelve una lista vacía, pero la mantendremos si la interfaz lo requiere.
            if (lotsDto == null)
            {
                return NotFound();
            }
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

        // [HttpPut("{id}")]
        // public async Task<IActionResult> Put(Guid id, [FromBody] UpdateLotDto updateDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }

        //     // CORRECCIÓN DE LA COHERENCIA DEL ID (Asumo que el DTO usa LotId)
        //     if (id != updateDto.Id)
        //     {
        //         return BadRequest(new { message = "ID mismatch between URL and body." });
        //     }

        //     // 1. Extracción del ID del token para auditoría/permisos
        //     var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
        //     {
        //         return Unauthorized();
        //     }

        //     try
        //     {
        //         await _lotService.UpdateLotAsync(updateDto, currentUserId);
        //         return NoContent();
        //     }
        //     catch (KeyNotFoundException)
        //     {
        //         return NotFound();
        //     }
        //     catch (UnauthorizedAccessException)
        //     {
        //         return Forbid();
        //     }
        // }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized();
            }

            try
            {
                bool deleted = await _lotService.SoftDeleteLotAsync(id, currentUserId);

                if (!deleted)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred during deletion.");
            }
        }
    }
}