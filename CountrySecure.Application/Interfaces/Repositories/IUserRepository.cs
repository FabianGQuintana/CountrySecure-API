using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);
        
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id); // soft delete
    }
}