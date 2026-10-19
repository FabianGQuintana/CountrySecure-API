using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>
    /// Permisos de ingreso. Los residentes gestionan los propios; Seguridad y Admin operan la portería.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/entry-permissions")]
    public class EntryPermissionsController : ControllerBase
    {
        private readonly IEntryPermissionService _permissionService;
        private readonly ICurrentUserService _currentUser;

        public EntryPermissionsController(IEntryPermissionService permissionService, ICurrentUserService currentUser)
        {
            _permissionService = permissionService;
            _currentUser = currentUser;
        }

        // -------------------------------------------------------------------
        // CONSULTA (un residente solo ve los suyos; lo resuelve el servicio)
        // -------------------------------------------------------------------

        [HttpGet]
        public async Task<ActionResult<PagedResult<EntryPermissionResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] PermissionType? type = null,
            [FromQuery] PermissionStatus? status = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? visitId = null,
            [FromQuery] Guid? orderId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _permissionService.GetPagedAsync(paging, type, status, userId, visitId, orderId, search, includeInactive));

        /// <summary>Permisos del usuario autenticado.</summary>
        [HttpGet("me")]
        public async Task<ActionResult<PagedResult<EntryPermissionResponseDto>>> GetMine(
            [FromQuery] PageQuery paging,
            [FromQuery] PermissionType? type = null,
            [FromQuery] PermissionStatus? status = null)
            => Ok(await _permissionService.GetPagedAsync(
                paging, type, status, _currentUser.GetRequiredUserId(), null, null, null, includeInactive: true));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<EntryPermissionResponseDto>> GetById(Guid id)
            => Ok(await _permissionService.GetByIdAsync(id));

        // -------------------------------------------------------------------
        // ESCRITURA (titular o Admin; lo resuelve el servicio)
        // -------------------------------------------------------------------

        [HttpPost]
        [Authorize(Roles = $"{Domain.Constants.RoleTypes.Admin},{Domain.Constants.RoleTypes.Resident}")]
        public async Task<ActionResult<EntryPermissionResponseDto>> Create([FromBody] CreateEntryPermissionDto dto)
        {
            var created = await _permissionService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = $"{Domain.Constants.RoleTypes.Admin},{Domain.Constants.RoleTypes.Resident}")]
        public async Task<ActionResult<EntryPermissionResponseDto>> Update(Guid id, [FromBody] UpdateEntryPermissionDto dto)
            => Ok(await _permissionService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Cancela el permiso (baja lógica). Solo si todavía no se usó.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = $"{Domain.Constants.RoleTypes.Admin},{Domain.Constants.RoleTypes.Resident}")]
        public async Task<ActionResult<EntryPermissionResponseDto>> Delete(Guid id)
            => Ok(await _permissionService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = $"{Domain.Constants.RoleTypes.Admin},{Domain.Constants.RoleTypes.Resident}")]
        public async Task<ActionResult<EntryPermissionResponseDto>> Restore(Guid id)
            => Ok(await _permissionService.RestoreAsync(id, _currentUser.GetRequiredUserId()));

        // -------------------------------------------------------------------
        // PORTERÍA (Seguridad o Admin)
        // -------------------------------------------------------------------

        /// <summary>Valida un QR y devuelve el estado del semáforo (entrada / salida).</summary>
        [HttpGet("validate")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<GateCheckResponseDto>> Validate([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("QR code value is required.");

            return Ok(await _permissionService.ValidateQrCodeAsync(code));
        }

        [HttpGet("today")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<PagedResult<EntryPermissionResponseDto>>> GetToday([FromQuery] PageQuery paging)
            => Ok(await _permissionService.GetTodayAsync(paging));

        [HttpGet("history")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<PagedResult<EntryPermissionResponseDto>>> GetHistory(
            [FromQuery] PageQuery paging,
            [FromQuery] string? search = null,
            [FromQuery] PermissionType? type = null)
            => Ok(await _permissionService.GetHistoryAsync(paging, search, type));

        [HttpPatch("{id:guid}/check-in")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<EntryPermissionResponseDto>> CheckIn(Guid id)
            => Ok(await _permissionService.RegisterCheckInAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPatch("{id:guid}/check-out")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<EntryPermissionResponseDto>> CheckOut(Guid id)
            => Ok(await _permissionService.RegisterCheckOutAsync(id, _currentUser.GetRequiredUserId()));

        /// <summary>Registra una entrada o salida. El guardia se toma del token, nunca del body.</summary>
        [HttpPost("movements")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<EntryPermissionResponseDto>> RegisterMovement([FromBody] RegisterMovementDto dto)
        {
            var guardId = _currentUser.GetRequiredUserId();

            return Ok(dto.IsEntry
                ? await _permissionService.RegisterCheckInAsync(dto.PermissionId, guardId)
                : await _permissionService.RegisterCheckOutAsync(dto.PermissionId, guardId));
        }
    }
}
