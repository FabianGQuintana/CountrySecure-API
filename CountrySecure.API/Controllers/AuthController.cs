

using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new {message = result.Message});
            }

            return Ok(result);
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                if (result.Message == "User not found")
                {
                    return NotFound(new { message = result.Message });
                }

                return Unauthorized(new { message = result.Message });
            }

            return Ok(result);
        }

    }
}