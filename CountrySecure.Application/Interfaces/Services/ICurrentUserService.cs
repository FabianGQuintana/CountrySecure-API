namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>
    /// Expone los datos del usuario autenticado (extraídos del JWT de la request actual).
    /// </summary>
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }

        /// <summary>
        /// Devuelve el Id del usuario o lanza <see cref="UnauthorizedAccessException"/> si no hay un usuario válido en el token.
        /// </summary>
        Guid GetRequiredUserId();
    }
}
