using System.Security.Claims;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ITokenService
    {
        /// <summary>Genera el access token (JWT) de un usuario.</summary>
        string GenerateToken(User user);

        /// <summary>Genera un refresh token opaco y aleatorio.</summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Valida la firma de un access token ignorando su vencimiento y devuelve sus claims.
        /// Lanza <see cref="Domain.Exceptions.AuthenticationFailedException"/> si el token es inválido.
        /// </summary>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
