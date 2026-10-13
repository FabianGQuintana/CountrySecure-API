using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TurnController : ControllerBase
    {
        private readonly ITurnService _turnService;
        private readonly ICurrentUserService _currentUser;

        public TurnController(ITurnService turnService, ICurrentUserService currentUser)
        {
            _turnService = turnService;
            _currentUser = currentUser;
        }

        // --- ESCRITURA ---

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTurnDto dto)
        {
            var turnDto = await _turnService.AddNewTurnAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = turnDto.Id }, turnDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateTurnDto updateDto)
        {
            var updatedTurnDto = await _turnService.UpdateTurnAsync(id, updateDto, _currentUser.GetRequiredUserId())
                                 ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");
            return Ok(updatedTurnDto);
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var updatedTurnDto = await _turnService.SoftDeleteTurnAsync(id, _currentUser.GetRequiredUserId())
                                 ?? throw new KeyNotFoundException($"Turn with ID {id} not found.");

            var action = updatedTurnDto.Status == "Active" ? "reactivated" : "deactivated";
            return Ok(new
            {
                Message = $"The Turn with ID {id} has been {action} successfully.",
                Turn = updatedTurnDto
            });
        }

        // --- CONSULTA ---

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _turnService.GetAllTurnsAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var turnDto = await _turnService.GetTurnByIdAsync(id);
            return turnDto == null ? NotFound() : Ok(turnDto);
        }

        // Reservas del usuario autenticado ("Mis reservas")
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
            => Ok(await _turnService.GetTurnsByUserIdAsync(_currentUser.GetRequiredUserId()));

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
            => Ok(await _turnService.GetTurnsByUserIdAsync(userId));

        [HttpGet("amenity/{amenityId:guid}")]
        public async Task<IActionResult> GetByAmenityId(Guid amenityId)
            => Ok(await _turnService.GetTurnsByAmenityIdAsync(amenityId));

        [HttpGet("range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("The start date cannot be after the end date.");

            return Ok(await _turnService.GetTurnsByDateRangeAsync(startDate, endDate));
        }
    }
}
