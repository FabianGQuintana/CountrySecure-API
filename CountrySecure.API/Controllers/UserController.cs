using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUser;

        public UsersController(IUserService userService, ICurrentUserService currentUser)
        {
            _userService = userService;
            _currentUser = currentUser;
        }

        /// <summary>Lista paginada de usuarios (?role, ?search, ?includeInactive). Solo Admin.</summary>
        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] string? role = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _userService.GetPagedAsync(paging, role, search, includeInactive));

        [HttpGet("me")]
        public async Task<ActionResult<UserResponseDto>> GetMe()
            => Ok(await _userService.GetByIdAsync(_currentUser.GetRequiredUserId()));

        /// <summary>Edición del propio perfil (sin Role ni Dni salvo que sea Admin; lo valida el servicio).</summary>
        [HttpPut("me")]
        public async Task<ActionResult<UserResponseDto>> UpdateMe([FromBody] UpdateUserDto dto)
            => Ok(await _userService.UpdateAsync(_currentUser.GetRequiredUserId(), dto));

        /// <summary>Elige uno de los avatares predefinidos (avatar-1 … avatar-5) o null para volver a las iniciales.</summary>
        [HttpPut("me/avatar")]
        public async Task<ActionResult<UserResponseDto>> UpdateMyAvatar([FromBody] UpdateAvatarDto dto)
            => Ok(await _userService.UpdateAvatarAsync(_currentUser.GetRequiredUserId(), dto.Avatar));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
        {
            // Un residente solo puede ver su propio usuario; Admin y Seguridad pueden ver cualquiera.
            if (_currentUser.Role == RoleTypes.Resident && _currentUser.UserId != id)
                throw new UnauthorizedAccessException("Solo podés ver tu propio usuario.");

            return Ok(await _userService.GetByIdAsync(id));
        }

        /// <summary>Admin: cualquier usuario. Resto: solo a sí mismo y sin campos sensibles (lo valida el servicio).</summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserResponseDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
            => Ok(await _userService.UpdateAsync(id, dto));

        /// <summary>Da de baja al usuario (baja lógica) y revoca sus sesiones.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<UserResponseDto>> Deactivate(Guid id)
            => Ok(await _userService.DeactivateAsync(id));

        /// <summary>Bloquea o desbloquea a un residente: mientras esté bloqueado, sus visitas no ingresan.</summary>
        [HttpPatch("{id:guid}/block")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<UserResponseDto>> SetBlocked(Guid id, [FromBody] BlockDto dto)
            => Ok(await _userService.SetBlockedAsync(id, dto));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<UserResponseDto>> Restore(Guid id)
            => Ok(await _userService.RestoreAsync(id));
    }
}
