using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
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

        public Task<User?> GetByIdAsync(Guid id)
            => _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByIdIncludingDeletedAsync(Guid id)
            => _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(string email)
            => _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public Task<bool> EmailExistsAsync(string email, Guid? exceptUserId = null)
            => _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email && u.Id != exceptUserId);

        public Task<bool> DniExistsAsync(int dni, Guid? exceptUserId = null)
            => _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Dni == dni && u.Id != exceptUserId);

        public Task<PagedResult<User>> GetPagedAsync(PageQuery paging, string? role, string? search, bool includeDeleted)
        {
            var query = _context.Users.AsNoTracking().IncludeDeleted(includeDeleted);

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => (u.Name + " " + u.Lastname).ToLower().Contains(term)
                                         || u.Email.ToLower().Contains(term)
                                         || u.Dni.ToString().Contains(term));
            }

            return query.OrderBy(u => u.Lastname).ThenBy(u => u.Name).ToPagedResultAsync(paging);
        }

        public async Task AddAsync(User user)
            => await _context.Users.AddAsync(user);

        public Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        // --- Tokens ---

        public Task<RefreshToken?> GetRefreshTokenAsync(string token)
            => _context.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == token);

        public async Task AddRefreshTokenAsync(RefreshToken token)
            => await _context.RefreshTokens.AddAsync(token);

        public Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task DeleteStaleRefreshTokensAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var usedBefore = now.AddHours(-1);

            // Solo se borran los que ya no pueden usarse; los tokens vigentes de otros dispositivos se conservan.
            var stale = _context.RefreshTokens.Where(rt => rt.UserId == userId
                             && (rt.Expires < now
                                 || rt.IsRevoked
                                 || (rt.IsUsed && rt.UsedAt != null && rt.UsedAt < usedBefore)));

            try
            {
                // Borrado directo en la base: si otra request concurrente ya borró las mismas filas, no falla
                // (cargar y borrar entidades lanzaba DbUpdateConcurrencyException y cortaba la sesión).
                await stale.ExecuteDeleteAsync();
            }
            catch (InvalidOperationException)
            {
                // Proveedores sin soporte de ExecuteDelete (base en memoria de los tests)
                _context.RefreshTokens.RemoveRange(await stale.ToListAsync());
            }
        }

        public async Task RevokeAllRefreshTokensAsync(Guid userId)
        {
            var active = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in active)
                token.IsRevoked = true;
        }
    }
}
