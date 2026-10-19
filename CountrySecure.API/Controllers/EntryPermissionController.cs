using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.EntryPermission;
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
    [Route("api/invitations")] // mismo recurso con el nombre del flujo de invitaciones
    public class EntryPermissionsController : ControllerBase
    {
        private readonly IEntryPermissionService _permissionService;
        private readonly IAccessControlService _accessControl;
        private readonly IPassDocumentService _passDocument;
        private readonly ICurrentUserService _currentUser;

        public EntryPermissionsController(
            IEntryPermissionService permissionService,
            IAccessControlService accessControl,
            IPassDocumentService passDocument,
            ICurrentUserService currentUser)
        {
            _permissionService = permissionService;
            _accessControl = accessControl;
            _passDocument = passDocument;
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
        // PASE DIGITAL (titular, Admin o Seguridad; lo resuelve el servicio)
        // -------------------------------------------------------------------

        /// <summary>Datos del pase (incluye el token que va dentro del QR).</summary>
        [HttpGet("{id:guid}/pass")]
        public async Task<ActionResult<PassDto>> GetPass(Guid id)
            => Ok(await _accessControl.GetPassAsync(id));

        /// <summary>Pase en PDF para descargar o compartir con la visita. El QR va en la esquina superior izquierda.</summary>
        [HttpGet("{id:guid}/pass/pdf")]
        public async Task<IActionResult> GetPassPdf(Guid id)
        {
            var pass = await _accessControl.GetPassAsync(id);
            return File(_passDocument.GeneratePdf(pass), "application/pdf", PassFileName(pass));
        }

        internal static string PassFileName(PassDto pass)
        {
            var safe = new string(pass.VisitorName.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray()).Trim().Replace(' ', '-');
            return $"pase-{(safe.Length > 0 ? safe : "visita")}.pdf";
        }

        // -------------------------------------------------------------------
        // PORTERÍA (Seguridad o Admin)
        // -------------------------------------------------------------------

        /// <summary>
        /// Escanea un pase: valida existencia, estado, ventana horaria y lista negra, y registra el ingreso
        /// (o la salida con action=CheckOut). Un rechazo responde 200 con success=false y el motivo; todo queda auditado.
        /// </summary>
        [HttpPost("scan")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<ActionResult<ScanResultDto>> Scan([FromBody] ScanRequestDto dto)
            => Ok(await _accessControl.ScanAsync(dto, _currentUser.GetRequiredUserId()));

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
