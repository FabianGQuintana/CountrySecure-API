using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly ICurrentUserService _currentUser;

        public AmenityController(IAmenityService amenityService, ICurrentUserService currentUser)
        {
            _amenityService = amenityService;
            _currentUser = currentUser;
        }

        // --- ESCRITURA (solo Admin) ---

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] AmenityCreateDto createDto)
        {
            var result = await _amenityService.AmenityCreateAsync(createDto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AmenityUpdateDto updateDto)
        {
            var result = await _amenityService.AmenityUpdateAsync(id, updateDto, _currentUser.GetRequiredUserId())
                         ?? throw new KeyNotFoundException($"Amenity with Id '{id}' not found.");
            return Ok(result);
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> SoftDeleteToggle(Guid id)
        {
            var updatedAmenityDto = await _amenityService.SoftDeleteToggleAsync(id, _currentUser.GetRequiredUserId())
                                    ?? throw new KeyNotFoundException($"Amenity con Id '{id}' no encontrado.");

            var action = updatedAmenityDto.Status == "Active" ? "reactivado" : "desactivado";
            return Ok(new
            {
                Message = $"La Amenity con ID {id} ha sido {action} exitosamente.",
                Amenity = updatedAmenityDto
            });
        }

        // --- CONSULTA ---

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(await _amenityService.GetByIdAsync(id));

        [HttpGet]
        public async Task<IActionResult> GetAllWithTurns([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _amenityService.GetAllAmenitiesWithTurnsAsync(pageNumber, pageSize));

        [HttpGet("name/{amenityName}")]
        public async Task<IActionResult> GetByName(string amenityName)
            => Ok(await _amenityService.GetAmenityByNameAsync(amenityName));

        [HttpGet("capacity")]
        public async Task<IActionResult> GetByCapacity([FromQuery] int minimumCapacity)
            => Ok(await _amenityService.GetAmenitiesByCapacityAsync(minimumCapacity));

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status)
            => Ok(await _amenityService.GetAmenitiesByStatusAsync(status));
    }
}
