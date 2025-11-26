using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);
        
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user); // soft delete

        Task SaveChangesAsync();
    }
}