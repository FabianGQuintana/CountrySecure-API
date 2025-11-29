

using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;

namespace CountrySecure.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CountrySecureDbContext _context;

        public IUserRepository Users { get; }

        public UnitOfWork(CountrySecureDbContext context, IUserRepository userRepository)
        {
            _context = context;
            Users = userRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}