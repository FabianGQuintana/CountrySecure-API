using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);
        Task<User?> ToggleActiveAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, string? role = null);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
        // Task<bool> DeleteAsync(Guid id); // soft delete
    
        // Tokens
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken token);
        Task UpdateRefreshTokenAsync(RefreshToken token);
    }
}