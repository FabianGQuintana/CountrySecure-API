using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CountrySecure.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        private SymmetricSecurityKey SigningKey
        {
            get
            {
                var key = _config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("JWT secret key is not configured (Jwt:Key).");

                return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            }
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,
                ValidateLifetime = false // se acepta un access token vencido: solo importa que la firma sea válida
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                throw new AuthenticationFailedException("Invalid access token.");
            }
        }
    }
}
