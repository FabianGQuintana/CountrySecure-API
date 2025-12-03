using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> ToggleActiveAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, string? role = null);
        
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id); // soft delete
    }
}