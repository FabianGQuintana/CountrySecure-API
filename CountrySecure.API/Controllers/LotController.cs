using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize(Policy = Policies.AdminOnly)]
    [Route("api/lots")]
    public class LotController : ControllerBase
    {
        private readonly ILotService _lotService;
        private readonly ICurrentUserService _currentUser;

        public LotController(ILotService lotService, ICurrentUserService currentUser)
        {
            _lotService = lotService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<LotResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] LotStatus? state = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _lotService.GetPagedAsync(paging, state, search, includeInactive));

        [HttpGet("block-names")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBlockNames()
            => Ok(await _lotService.GetBlockNamesAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LotResponseDto>> GetById(Guid id)
            => Ok(await _lotService.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<LotResponseDto>> Create([FromBody] CreateLotDto dto)
        {
            var created = await _lotService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.LotId }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<LotResponseDto>> Update(Guid id, [FromBody] UpdateLotDto dto)
            => Ok(await _lotService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Baja lógica.</summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<LotResponseDto>> Delete(Guid id)
            => Ok(await _lotService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        public async Task<ActionResult<LotResponseDto>> Restore(Guid id)
            => Ok(await _lotService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
