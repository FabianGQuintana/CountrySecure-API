using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
    }
}