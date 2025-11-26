using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);
        
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id); // soft delete

        Task SaveChangesAsync();
    }
}