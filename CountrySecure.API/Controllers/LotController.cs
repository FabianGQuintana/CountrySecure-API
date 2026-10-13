using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[Controller]")]
    public class LotController : ControllerBase
    {
        private readonly ILotService _lotService;
        private readonly ICurrentUserService _currentUser;

        public LotController(ILotService lotService, ICurrentUserService currentUser)
        {
            _lotService = lotService;
            _currentUser = currentUser;
        }

        // --- CONSULTA ---

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _lotService.GetAllLotsAsync(pageNumber, pageSize));

        [HttpGet("block-names")]
        public async Task<IActionResult> GetAllBlockNames()
            => Ok(await _lotService.GetAllBlockNamesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lotDto = await _lotService.GetLotByIdAsync(id);
            return lotDto == null ? NotFound() : Ok(lotDto);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetLotsByStatus([FromQuery] LotStatus status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _lotService.GetLotsByStatusAsync(status, pageNumber, pageSize));

        // --- ESCRITURA (solo Admin) ---

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Post([FromBody] CreateLotDto dto)
        {
            var lotDto = await _lotService.AddNewLotAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = lotDto.LotId }, lotDto);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateLotDto updatedto)
            => Ok(await _lotService.UpdateAsync(updatedto, id, _currentUser.GetRequiredUserId()));

        [HttpPatch("{id:guid}/SoftDelete")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> SoftDeleteToggle(Guid id)
        {
            var updatedLotDto = await _lotService.SoftDeleteToggleAsync(id, _currentUser.GetRequiredUserId())
                                ?? throw new KeyNotFoundException($"Lot with ID {id} not found.");

            var action = updatedLotDto.Status == "Active" ? "reactivated" : "deactivated";
            return Ok(new
            {
                Message = $"The Lot with ID {id} has been {action} successfully.",
                Lot = updatedLotDto
            });
        }
    }
}
