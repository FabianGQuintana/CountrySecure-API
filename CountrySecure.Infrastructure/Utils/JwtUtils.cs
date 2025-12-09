using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CountrySecure.Infrastructure.Utils
{
    public class JwtUtils
    {
        private readonly string? _secret;

        public JwtUtils(IConfiguration configuration)
        {
            // Lee la clave secreta desde appsettings.json -> "Jwt:Key"
            _secret = configuration["Jwt:Key"];
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret ?? throw new InvalidOperationException("JWT secret key is not configured."))),
                ValidateLifetime = false // <-- permite validar tokens expirados
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            // variable obligatoria para el método ValidateToken
            SecurityToken validatedToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

            return principal;
        }


        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
        }

        public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
    }
}
