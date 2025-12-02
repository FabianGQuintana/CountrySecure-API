
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
        // 1. Método: GET /api/Lot?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult>  GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var lotsDto = await _lotService.GetAllLotsAsync(pageNumber, pageSize);
            return Ok(lotsDto); // 200 OK
        }


        // 2. Método: GET /api/Lot/block-names
        [HttpGet("block-names")]
        public async Task<IActionResult> GetAllBlockNames()
        {
            var blockNames = await _lotService.GetAllBlockNamesAsync();
            return Ok(blockNames); // 200 OK
        }


        // 3. Método: GET /api/Lot/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lotDto = await _lotService.GetLotByIdAsync(id);
            if (lotDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(lotDto); // 200 OK
        }



        // 4. Método: GET /api/Lot/status?status=Available&pageNumber=1&pageSize=10
        [HttpGet("status")]
        
        public async Task<IActionResult> GetLotsByStatus([FromQuery] LotStatus status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var lotsDto = await _lotService.GetLotsByStatusAsync(status, pageNumber, pageSize);
            if(lotsDto == null)
            {
                return NotFound(); // 404 Not Found
            }
            return Ok(lotsDto); // 200 OK
        }

        // --- MÉTODOS DE ESCRITURA (POST, PUT, DELETE) ---

        // 5. Método: POST /api/Lot (Creación)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateLotDto dto)
        {
            // Validar el DTO de entrada
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            var lotDto = await _lotService.AddNewLotAsync(dto);

            // 201 Created
            // Asumimos que LotResponseDto tiene la propiedad Id
            return CreatedAtAction(nameof(GetById), new { id = lotDto.LotId }, lotDto);
        }

        // 6. Método: PUT /api/Lot/{id} (Actualización Segura)
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateLotDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // VALIDACIÓN DE COHERENCIA: ID de la URL vs ID del DTO
            if (id != updateDto.Id)
            {
                return BadRequest(new { message = "ID mismatch between URL and body." });
            }

            try
            {
                // currentUserId extraído del token
                Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await _lotService.UpdateLotAsync(updateDto, currentUserId);

                return NoContent(); // 204 No Content (Actualización exitosa)
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // 404 Not Found si el Lote no existe
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // 403 Forbidden
            }
        }

        // 7. Método: DELETE /api/Lot/{id} (Baja Lógica Segura)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            // 1. EXTRACCIÓN DEL ID SEGURO DEL TOKEN
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized();
            }

            try
            {
                // 2. Llama al servicio, pasando el ID del recurso y el ID del usuario logueado (para verificación de permisos en el servicio)
                bool deleted = await _lotService.SoftDeleteLotAsync(id, currentUserId);

                if (!deleted)
                {
                    return NotFound(); // 404 Not Found si el Lote no existe
                }
                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // 403 Forbidden
            }
            catch (Exception)
            {
                // Capturar errores generales, ej. error de base de datos
                return StatusCode(500, "An error occurred during deletion.");
            }
        }
    }
}
