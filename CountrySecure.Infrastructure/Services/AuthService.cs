using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Utils;

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
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
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
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }


            var token = _tokenService.GenerateToken(user);
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
                AccessToken = token,
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


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Primero se busca el Refresh token en la DB
            var storedToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked)
                throw new Exception("Invalid refresh token");

            if (storedToken.Expires < DateTime.UtcNow)
                throw new Exception("Refresh Token expired");

            // 2. Extraer el UserId del accesToken que expiró
            var principal = _jwtUtils.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = Guid.Parse(principal.Claims.First(c => c.Type == "id").Value);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");


            // 3. marcar Refresh token previo como usado
            storedToken.IsUsed = true;
            await _unitOfWork.Users.UpdateRefreshTokenAsync(storedToken);

            // 4. Generar nuevos tokens
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
            await _unitOfWork.SaveChangesAsync();

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