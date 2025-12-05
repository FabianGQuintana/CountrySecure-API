using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.DTOs.Amenity;

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

    
        [HttpPost]
        [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] 
        public async Task<IActionResult> Create(AmenityCreateDto dto)
        {
            try
            {
                var response = await _amenityService.AmenityCreateAsync(dto);
                
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al crear la amenidad.");
            }
        }

        [HttpGet("{id:guid}")] 
        [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var response = await _amenityService.GetByIdAsync(id);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Amenidad con Id '{id}' no encontrada.");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AmenityResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100)
        {
            var response = await _amenityService.GetAllAsync(page, size);
            return Ok(response);
        }

        
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AmenityResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, AmenityUpdateDto dto)
        {
            try
            {
                var response = await _amenityService.AmenityUpdateAsync(id, dto);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Amenidad con Id '{id}' no encontrada para actualizar.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al actualizar la amenidad.");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _amenityService.DeleteAmenityAsync(id);

                if (!success) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Amenidad con Id '{id}' no encontrada para eliminar.");
            }
        }
    }
}