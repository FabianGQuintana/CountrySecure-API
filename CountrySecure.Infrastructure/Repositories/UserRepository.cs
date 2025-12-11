using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Constants;
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
                .AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            return await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        // public async Task<bool> DeleteAsync(Guid id)
        // {
        //     var user = await GetByIdAsync(id);
        //     if (user == null) return false;

        //     user.DeletedAt = DateTime.UtcNow;
        //     user.EntryPermissionState = "Inactive"; // soft delete
        //     user.Active = false;
        //     _context.Users.Update(user);

        //     return true;
        // }

        public async Task<User?> ToggleActiveAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.Active = !user.Active;

            user.Status = user.Status == "Active" ? "Inactive" : "Active";

            if (!user.Active)
                user.DeletedAt = DateTime.UtcNow;
            else
                user.DeletedAt = null;

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

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task DeleteRefreshTokenAsync(Guid userId, TimeSpan maxAge)
        {
            var cutOfDate = DateTime.UtcNow - maxAge;

            var tokensToDelete = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.CreatedAt < cutOfDate)
                .ToListAsync();

            if (tokensToDelete.Any())
            {
                _context.RefreshTokens.RemoveRange(tokensToDelete);
            } 

        }
    }
}