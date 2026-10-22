using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class TurnRepository : GenericRepository<Turn>, ITurnRepository
    {
        public TurnRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        public Task<Turn?> GetByIdWithIncludesAsync(Guid id, bool includeDeleted = false)
            => Set.IncludeDeleted(includeDeleted)
                  .Include(t => t.User)
                  .Include(t => t.Amenity)
                  .FirstOrDefaultAsync(t => t.Id == id);

        public Task<PagedResult<Turn>> GetPagedAsync(
            PageQuery paging, Guid? userId, Guid? amenityId, DateTime? from, DateTime? to, TurnStatus? status, bool includeDeleted)
        {
            var query = Set.AsNoTracking()
                           .IncludeDeleted(includeDeleted)
                           .Include(t => t.User)
                           .Include(t => t.Amenity)
                           .AsQueryable();

            if (userId.HasValue) query = query.Where(t => t.UserId == userId.Value);
            if (amenityId.HasValue) query = query.Where(t => t.AmenityId == amenityId.Value);
            if (from.HasValue) query = query.Where(t => t.EndTime >= from.Value);
            if (to.HasValue) query = query.Where(t => t.StartTime <= to.Value);
            if (status.HasValue) query = query.Where(t => t.TurnStatus == status.Value);

            return query.OrderByDescending(t => t.StartTime).ToPagedResultAsync(paging);
        }

        public Task<List<Turn>> GetActiveOverlappingTurnsAsync(Guid amenityId, DateTime start, DateTime end, Guid? excludeTurnId = null)
        {
            // Dos intervalos [a,b) y [c,d) se solapan si a < d && c < b
            return Set.AsNoTracking()
                      .Where(t => t.AmenityId == amenityId
                                  && t.TurnStatus != TurnStatus.Cancelled
                                  && t.StartTime < end
                                  && start < t.EndTime
                                  && (excludeTurnId == null || t.Id != excludeTurnId))
                      .ToListAsync();
        }
    }
}
