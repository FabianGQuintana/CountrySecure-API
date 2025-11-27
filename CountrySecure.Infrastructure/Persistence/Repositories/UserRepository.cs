using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            user.IsDeleted = true; // soft delete
            _context.Users.Update(user);

            return true;
        }
    }

}