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


        // Ventana en la que un refresh token ya consumido todavía devuelve el mismo reemplazo.
        // Cubre requests simultáneas (Server Components, prefetch, varias pestañas) que usan el mismo token.
        private static readonly TimeSpan RefreshGracePeriod = TimeSpan.FromSeconds(30);

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Buscar el refresh token en la base de datos
            var storedToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked)
                throw new SecurityTokenException("Invalid refresh token.");

            if (storedToken.Expires < DateTime.UtcNow)
                throw new SecurityTokenExpiredException("Refresh token expired.");

            // 2. Validar el access token (vencido o no) y que pertenezca al mismo usuario
            var principal = _jwtUtils.GetPrincipalFromExpiredToken(request.AccessToken);

            var userIdClaim = principal.Claims.FirstOrDefault(c =>
                c.Type == "id" ||
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier
            );

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new SecurityTokenException("User ID not found in access token claims.");

            if (storedToken.UserId != userId)
                throw new SecurityTokenException("Refresh token does not belong to this user.");

            // 3. Usuario existente y activo (un usuario dado de baja no puede renovar su sesión)
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.Active || user.DeletedAt != null)
                throw new SecurityTokenException("User not found or inactive.");

            // 4. Token ya consumido: dentro de la ventana de gracia se devuelve el mismo reemplazo
            if (storedToken.IsUsed)
            {
                var withinGrace = storedToken.UsedAt.HasValue
                                  && DateTime.UtcNow - storedToken.UsedAt.Value <= RefreshGracePeriod
                                  && !string.IsNullOrEmpty(storedToken.ReplacedByToken);

                if (!withinGrace)
                    throw new SecurityTokenException("Invalid refresh token.");

                var replacement = await _unitOfWork.Users.GetRefreshTokenAsync(storedToken.ReplacedByToken!);
                if (replacement == null || replacement.IsRevoked || replacement.Expires < DateTime.UtcNow)
                    throw new SecurityTokenException("Invalid refresh token.");

                return BuildAuthResponse(user, _tokenService.GenerateToken(user), replacement.Token);
            }

            // 5. Rotación normal: se consume el token y se emite uno nuevo
            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            storedToken.IsUsed = true;
            storedToken.UsedAt = DateTime.UtcNow;
            storedToken.ReplacedByToken = newRefreshToken;
            await _unitOfWork.Users.UpdateRefreshTokenAsync(storedToken);

            // Limpieza: solo tokens vencidos, revocados o consumidos hace tiempo
            await _unitOfWork.Users.DeleteStaleRefreshTokensAsync(userId);

            await _unitOfWork.Users.AddRefreshTokenAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            return BuildAuthResponse(user, newAccessToken, newRefreshToken);
        }

        private static AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken) => new()
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddHours(8),
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Lastname = user.Lastname,
            Role = user.Role
        };

    }
}