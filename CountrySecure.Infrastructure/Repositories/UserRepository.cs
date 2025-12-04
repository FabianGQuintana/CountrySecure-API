using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Persistence.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CountrySecureDbContext _context;

        public UserRepository(CountrySecureDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                // .Where(UserPredicates.NotDeleted)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize, string? role = null)
        {

            var query = _context.Users
                .Where(UserPredicates.NotDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            user.DeletedAt = DateTime.UtcNow; // soft delete
            _context.Users.Update(user);

            return true;
        }

        public async Task<User?> ToggleActiveAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.Active = !user.Active;
            user.UpdatedAt = DateTime.UtcNow;

            return user; // EF ya trackea esta entidad
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(UserPredicates.NotDeleted)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}