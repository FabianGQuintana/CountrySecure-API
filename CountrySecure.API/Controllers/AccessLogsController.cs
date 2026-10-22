using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Auditoría de portería: historial de cada escaneo (aprobado o rechazado).</summary>
    [ApiController]
    [Authorize(Policy = Policies.SecurityStaff)]
    [Route("api/access-logs")]
    public class AccessLogsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;

        public AccessLogsController(IAccessControlService accessControl) => _accessControl = accessControl;

        [HttpGet]
        public async Task<ActionResult<PagedResult<AccessLogResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] Guid? permissionId = null,
            [FromQuery] ScanOutcome? outcome = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
            => Ok(await _accessControl.GetLogsAsync(paging, permissionId, outcome, from, to));
    }
}
