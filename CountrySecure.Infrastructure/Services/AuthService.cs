using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Utils;
using Microsoft.IdentityModel.Tokens;

namespace CountrySecure.Infrastructure.Services
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        private readonly JwtUtils _jwtUtils;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            JwtUtils jwtUtils
        )
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _jwtUtils = jwtUtils;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already registered"
                };
            }


            var user = new User
            {
                Name = dto.Name,
                Lastname = dto.Lastname,
                Dni = dto.Dni,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 10),
                Role = dto.Role,
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                Role = user.Role
            };

        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                return new AuthResponseDto { Success = false, Message = "User not found" };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return new AuthResponseDto { Success = false, Message = "Incorrect password" };
            }

            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                IsRevoked = false
            };

            await _unitOfWork.Users.AddRefreshTokenAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddHours(8),
                UserId = user.Id,
                Email = user.Email,
                Lastname = user.Lastname,
                Name = user.Name,
                Role = user.Role
            };
        }


        public async Task LogoutAsync(string refreshToken)
        {
            var storedToken = await _unitOfWork.Users.GetRefreshTokenAsync(refreshToken);

            if (storedToken == null) return;

            storedToken.IsRevoked = true;
            await _unitOfWork.Users.UpdateRefreshTokenAsync(storedToken);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return (false, "Usuario no encontrado.");

            // Verificar password actual
            if (!_jwtUtils.VerifyPassword(dto.CurrentPassword, user.Password))
                return (false, "Current password is incorrect.");

            // Hashear nueva
            user.Password = _jwtUtils.HashPassword(dto.NewPassword);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Buscar el refresh token en la base de datos
            var storedToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked)
                throw new InvalidOperationException("Invalid refresh token.");

            if (storedToken.Expires < DateTime.UtcNow)
                throw new SecurityTokenExpiredException("Refresh token expired.");

            // 2. Obtener los claims del access token (vencido o no)
            var principal = _jwtUtils.GetPrincipalFromExpiredToken(request.AccessToken);

            // Extraer el UserId del token, tolerando varios nombres de claim
            var userIdClaim = principal.Claims.FirstOrDefault(c =>
                c.Type == "id" ||
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null)
                throw new InvalidOperationException("User ID not found in access token claims.");

            var userId = Guid.Parse(userIdClaim.Value);

            // 3. Obtener el usuario de la DB
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Validar que el refresh token pertenece a ese usuario
            if (storedToken.UserId != user.Id)
                throw new InvalidOperationException("Refresh token does not belong to this user.");

            // 4. Marcar el refresh token previo como usado
            storedToken.IsUsed = true;
            await _unitOfWork.Users.UpdateRefreshTokenAsync(storedToken);

            // Borrar los RefreshToken de la DB que tengan mas de 24hr
            await _unitOfWork.Users.DeleteRefreshTokenAsync(userId, TimeSpan.FromHours(24));

            // 5. Generar nuevos tokens
            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddRefreshTokenAsync(newRefreshTokenEntity);

            // Se guardan todos los cambios
            await _unitOfWork.SaveChangesAsync();

            // 6. Respuesta al cliente
            return new AuthResponseDto
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddHours(8),
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Lastname = user.Lastname,
                Role = user.Role
            };
        }

    }
}