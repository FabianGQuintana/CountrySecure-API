using System.Security.Claims;
using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Exceptions;

namespace CountrySecure.Application.Services.Auth
{
    /// <summary>
    /// Registro, login, rotación de refresh tokens y cambio de contraseña.
    /// Depende solo de abstracciones: el hash (<see cref="IPasswordHasher"/>) y los JWT
    /// (<see cref="ITokenService"/>) se implementan en Infrastructure.
    /// </summary>
    public class AuthService : IAuthService
    {
        // Ventana en la que un refresh token ya consumido todavía devuelve el mismo reemplazo.
        // Cubre requests simultáneas (Server Components, prefetch, varias pestañas) que usan el mismo token.
        private static readonly TimeSpan RefreshGracePeriod = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
        private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(8);

        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
                return new AuthResponseDto { Success = false, Message = "El email ya está registrado." };

            if (await _unitOfWork.Users.DniExistsAsync(dto.Dni))
                return new AuthResponseDto { Success = false, Message = "El DNI ya está registrado." };

            var user = new User
            {
                Name = dto.Name,
                Lastname = dto.Lastname,
                Dni = dto.Dni,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = _passwordHasher.Hash(dto.Password),
                Role = dto.Role,
            };

            await _unitOfWork.Users.AddAsync(user);
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
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

            // Mismo resultado para "no existe" y "clave incorrecta" (no permite enumerar usuarios)
            if (user == null || !_passwordHasher.Verify(dto.Password, user.Password))
            {
                return new AuthResponseDto { Success = false, Message = "Email o contraseña incorrectos." };
            }

            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _unitOfWork.Users.AddRefreshTokenAsync(NewRefreshToken(user.Id, refreshToken));
            await _unitOfWork.SaveChangesAsync();

            return BuildAuthResponse(user, accessToken, refreshToken);
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
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return (false, "Usuario no encontrado.");

            if (!_passwordHasher.Verify(dto.CurrentPassword, user.Password))
                return (false, "La contraseña actual es incorrecta.");

            user.Password = _passwordHasher.Hash(dto.NewPassword);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        /// <summary>
        /// Renueva la sesión. Un refresh token inválido, vencido, revocado o ajeno es un caso esperado
        /// (sesión vieja en el navegador): se devuelve <c>Success = false</c> en vez de propagar la
        /// excepción, así el controlador responde 401 sin ensuciar el log con "unhandled exception".
        /// </summary>
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                return await RotateRefreshTokenAsync(request);
            }
            catch (AuthenticationFailedException ex)
            {
                return new AuthResponseDto { Success = false, Message = ex.Message };
            }
        }

        private async Task<AuthResponseDto> RotateRefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Buscar el refresh token en la base de datos
            var storedToken = await _unitOfWork.Users.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked)
                throw new AuthenticationFailedException("La sesión no es válida. Volvé a iniciar sesión.");

            if (storedToken.Expires < DateTime.UtcNow)
                throw new AuthenticationFailedException("La sesión expiró. Volvé a iniciar sesión.");

            // 2. Validar el access token (vencido o no) y que pertenezca al mismo usuario
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            var userIdClaim = principal.Claims.FirstOrDefault(c =>
                c.Type == "id" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new AuthenticationFailedException("La sesión no es válida. Volvé a iniciar sesión.");

            if (storedToken.UserId != userId)
                throw new AuthenticationFailedException("La sesión no es válida. Volvé a iniciar sesión.");

            // 3. Usuario existente y activo (un usuario dado de baja no puede renovar su sesión)
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new AuthenticationFailedException("El usuario no existe o está dado de baja.");

            // 4. Token ya consumido: dentro de la ventana de gracia se devuelve el mismo reemplazo
            if (storedToken.IsUsed)
            {
                var withinGrace = storedToken.UsedAt.HasValue
                                  && DateTime.UtcNow - storedToken.UsedAt.Value <= RefreshGracePeriod
                                  && !string.IsNullOrEmpty(storedToken.ReplacedByToken);

                if (!withinGrace)
                    throw new AuthenticationFailedException("La sesión no es válida. Volvé a iniciar sesión.");

                var replacement = await _unitOfWork.Users.GetRefreshTokenAsync(storedToken.ReplacedByToken!);
                if (replacement == null || replacement.IsRevoked || replacement.Expires < DateTime.UtcNow)
                    throw new AuthenticationFailedException("La sesión no es válida. Volvé a iniciar sesión.");

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

            await _unitOfWork.Users.AddRefreshTokenAsync(NewRefreshToken(user.Id, newRefreshToken));
            await _unitOfWork.SaveChangesAsync();

            return BuildAuthResponse(user, newAccessToken, newRefreshToken);
        }

        private static RefreshToken NewRefreshToken(Guid userId, string token) => new()
        {
            Token = token,
            UserId = userId,
            Expires = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        };

        private static AuthResponseDto BuildAuthResponse(User user, string accessToken, string refreshToken) => new()
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.Add(AccessTokenLifetime),
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            Lastname = user.Lastname,
            Role = user.Role
        };
    }
}
