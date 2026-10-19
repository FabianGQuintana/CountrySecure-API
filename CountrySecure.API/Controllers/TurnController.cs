using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Reservas de amenities. Un residente solo accede a las propias.</summary>
    [ApiController]
    [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Resident}")]
    [Route("api/turns")]
    public class TurnController : ControllerBase
    {
        private readonly ITurnService _turnService;
        private readonly ICurrentUserService _currentUser;

        public TurnController(ITurnService turnService, ICurrentUserService currentUser)
        {
            _turnService = turnService;
            _currentUser = currentUser;
        }

        /// <summary>Reservas del usuario autenticado ("Mis reservas").</summary>
        [HttpGet("me")]
        public async Task<ActionResult<PagedResult<TurnResponseDto>>> GetMine(
            [FromQuery] PageQuery paging, [FromQuery] TurnStatus? status = null)
            => Ok(await _turnService.GetPagedAsync(paging, null, null, null, null, status, onlyMine: true, includeInactive: true));

        /// <summary>
        /// Listado con filtros (?userId, ?amenityId, ?from, ?to, ?status). Un residente ve solo las suyas.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<TurnResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? amenityId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] TurnStatus? status = null,
            [FromQuery] bool includeInactive = false)
        {
            if (from > to)
                throw new ArgumentException("The start date cannot be after the end date.");

            return Ok(await _turnService.GetPagedAsync(paging, userId, amenityId, from, to, status, onlyMine: false, includeInactive));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TurnResponseDto>> GetById(Guid id)
            => Ok(await _turnService.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<TurnResponseDto>> Create([FromBody] CreateTurnDto dto)
        {
            var created = await _turnService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TurnResponseDto>> Update(Guid id, [FromBody] UpdateTurnDto dto)
            => Ok(await _turnService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Cancela la reserva (baja lógica).</summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<TurnResponseDto>> Delete(Guid id)
            => Ok(await _turnService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        public async Task<ActionResult<TurnResponseDto>> Restore(Guid id)
            => Ok(await _turnService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
