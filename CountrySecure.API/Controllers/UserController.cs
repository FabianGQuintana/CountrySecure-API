using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUser;

        public UsersController(IUserService userService, ICurrentUserService currentUser)
        {
            _userService = userService;
            _currentUser = currentUser;
        }

        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? role = null)
            => Ok(await _userService.GetAllAsync(page, size, role));

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
            => Ok(await _userService.GetByIdAsync(_currentUser.GetRequiredUserId()));

        // Edición del propio perfil (sin Role ni Dni salvo que sea Admin; lo valida el servicio).
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto dto)
            => Ok(await _userService.UpdateUserAsync(_currentUser.GetRequiredUserId(), dto));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Un residente solo puede ver su propio usuario; Admin y Security pueden ver cualquiera.
            if (_currentUser.Role == RoleTypes.Resident && _currentUser.UserId != id)
                throw new UnauthorizedAccessException("You can only view your own user.");

            return Ok(await _userService.GetByIdAsync(id));
        }

        // Admin: cualquier usuario. Resto: solo a sí mismo y sin campos sensibles (validado en UserService).
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
            => Ok(await _userService.UpdateUserAsync(id, dto));

        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var updatedUser = await _userService.ToggleActiveAsync(id)
                              ?? throw new KeyNotFoundException($"User with id {id} not found");
            return Ok(updatedUser);
        }
    }
}
