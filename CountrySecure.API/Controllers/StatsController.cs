using CountrySecure.API.Authorization;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>
    /// KPIs reales para los dashboards de cada rol.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        private readonly ICurrentUserService _currentUser;

        public StatsController(IStatsService statsService, ICurrentUserService currentUser)
        {
            _statsService = statsService;
            _currentUser = currentUser;
        }

        // GET /api/stats -> devuelve los KPIs correspondientes al rol del usuario autenticado
        [HttpGet]
        public async Task<IActionResult> GetForCurrentUser()
        {
            return _currentUser.Role switch
            {
                RoleTypes.Admin => Ok(await _statsService.GetAdminStatsAsync()),
                RoleTypes.Security => Ok(await _statsService.GetSecurityStatsAsync()),
                RoleTypes.Resident => Ok(await _statsService.GetResidentStatsAsync(_currentUser.GetRequiredUserId())),
                _ => throw new UnauthorizedAccessException("Unknown role.")
            };
        }

        [HttpGet("admin")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAdmin()
            => Ok(await _statsService.GetAdminStatsAsync());

        [HttpGet("security")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> GetSecurity()
            => Ok(await _statsService.GetSecurityStatsAsync());

        [HttpGet("resident")]
        public async Task<IActionResult> GetResident()
            => Ok(await _statsService.GetResidentStatsAsync(_currentUser.GetRequiredUserId()));
    }
}
