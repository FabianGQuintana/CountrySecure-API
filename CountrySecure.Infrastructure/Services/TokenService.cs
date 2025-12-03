

using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace CountrySecure.Infrastructure.Services
{
    public class TokenService : ITokenService
    {

        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            // var claims
        }
    }
}