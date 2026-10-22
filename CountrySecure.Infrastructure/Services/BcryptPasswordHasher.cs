using CountrySecure.Application.Interfaces.Services;

namespace CountrySecure.Infrastructure.Services
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 10;

        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
