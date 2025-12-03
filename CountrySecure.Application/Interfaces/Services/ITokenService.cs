using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}