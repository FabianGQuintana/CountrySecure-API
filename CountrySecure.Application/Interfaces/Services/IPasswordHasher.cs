namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>
    /// Abstracción del hash de contraseñas. La implementación (BCrypt) vive en Infrastructure.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
}
