using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        /// <summary>Usuario activo (tracked, listo para modificar).</summary>
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByIdIncludingDeletedAsync(Guid id);

        /// <summary>Usuario activo por email (para el login).</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>El email y el DNI son únicos en toda la tabla, incluidos los usuarios dados de baja.</summary>
        Task<bool> EmailExistsAsync(string email, Guid? exceptUserId = null);
        Task<bool> DniExistsAsync(int dni, Guid? exceptUserId = null);

        Task<PagedResult<User>> GetPagedAsync(PageQuery paging, string? role, string? search, bool includeDeleted);

        Task AddAsync(User user);
        Task UpdateAsync(User user);

        // Tokens
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken token);
        Task UpdateRefreshTokenAsync(RefreshToken token);

        /// <summary>Elimina los refresh tokens del usuario que ya no sirven (vencidos, revocados o consumidos hace tiempo).</summary>
        Task DeleteStaleRefreshTokensAsync(Guid userId);

        /// <summary>Revoca todos los refresh tokens vigentes del usuario (p. ej. al darlo de baja).</summary>
        Task RevokeAllRefreshTokensAsync(Guid userId);

        // Recuperación de contraseña
        Task AddPasswordResetTokenAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(string tokenHash);
        Task<int> CountPasswordResetTokensSinceAsync(Guid userId, DateTime since);

        /// <summary>Marca como usados los enlaces pendientes del usuario (al pedir uno nuevo).</summary>
        Task InvalidatePasswordResetTokensAsync(Guid userId);
    }
}
