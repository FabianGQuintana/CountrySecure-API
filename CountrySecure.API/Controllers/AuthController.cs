using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUser;

        public AuthController(IAuthService authService, IUserService userService, ICurrentUserService currentUser)
        {
            _authService = authService;
            _userService = userService;
            _currentUser = currentUser;
        }

        // Solo un Admin puede dar de alta cuentas (el primer Admin se crea por seed).
        [HttpPost("register")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return CreatedAtAction(nameof(GetUserById), new { id = result.UserId }, result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            // Mismo 401 para email inexistente o contraseña incorrecta (evita enumerar usuarios)
            if (!result.Success)
                return Unauthorized(new { message = "Email o contraseña incorrectos." });

            return Ok(result);
        }

        // AllowAnonymous: al cerrar sesión el access token puede estar vencido; el refresh token es el secreto.
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { message = "Sesión cerrada." });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            // Refresh token inválido, vencido o revocado: 401 limpio (el front borra la sesión y manda a /login)
            if (!result.Success)
                return Unauthorized(new { message = result.Message ?? "La sesión no es válida. Volvé a iniciar sesión." });

            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = await _authService.ChangePasswordAsync(_currentUser.GetRequiredUserId(), dto);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "La contraseña se actualizó correctamente." });
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetUserById(Guid id)
            => Ok(await _userService.GetByIdAsync(id));
    }
}
