using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Visitantes (personas que ingresan a la comunidad).</summary>
    [ApiController]
    [Authorize]
    [Route("api/visits")]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        private readonly ICurrentUserService _currentUser;

        public VisitController(IVisitService visitService, ICurrentUserService currentUser)
        {
            _visitService = visitService;
            _currentUser = currentUser;
        }

        /// <summary>Lista paginada (?dni, ?search). Solo un Admin puede pedir las inactivas.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<VisitResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] int? dni = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _visitService.GetPagedAsync(paging, dni, search, includeInactive && _currentUser.IsAdmin));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VisitResponseDto>> GetById(Guid id)
            => Ok(await _visitService.GetByIdAsync(id));

        /// <summary>La visita junto con todos sus permisos.</summary>
        [HttpGet("{id:guid}/with-permits")]
        [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Security}")]
        public async Task<ActionResult<VisitWithPermitsDto>> GetWithPermits(Guid id)
            => Ok(await _visitService.GetWithPermitsAsync(id));

        /// <summary>Solo los permisos vigentes (pendientes) de la visita.</summary>
        [HttpGet("{id:guid}/permits/valid")]
        [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Security}")]
        public async Task<ActionResult<IReadOnlyList<VisitEntryPermissionDto>>> GetValidPermits(Guid id)
            => Ok(await _visitService.GetValidPermitsAsync(id));

        [HttpPost]
        [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Resident}")]
        public async Task<ActionResult<VisitResponseDto>> Create([FromBody] CreateVisitDto dto)
        {
            var created = await _visitService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.VisitId }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Resident}")]
        public async Task<ActionResult<VisitResponseDto>> Update(Guid id, [FromBody] UpdateVisitDto dto)
            => Ok(await _visitService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Baja lógica.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<VisitResponseDto>> Delete(Guid id)
            => Ok(await _visitService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        /// <summary>Lista negra: bloquea o desbloquea al visitante (Admin o Seguridad).</summary>
        [HttpPatch("{id:guid}/block")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<VisitResponseDto>> SetBlocked(Guid id, [FromBody] BlockDto dto)
            => Ok(await _visitService.SetBlockedAsync(id, dto, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<VisitResponseDto>> Restore(Guid id)
            => Ok(await _visitService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
