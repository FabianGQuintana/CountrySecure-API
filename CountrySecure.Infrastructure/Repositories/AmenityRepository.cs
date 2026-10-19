using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
    {
        public AmenityRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        public Task<Amenity?> GetByNameAsync(string amenityName)
            => Set.FirstOrDefaultAsync(a => a.AmenityName == amenityName);

        public Task<PagedResult<Amenity>> GetPagedAsync(PageQuery paging, string? search, int? minCapacity, bool includeDeleted)
        {
            var query = Set.AsNoTracking().IncludeDeleted(includeDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => a.AmenityName.ToLower().Contains(term) || a.Description.ToLower().Contains(term));
            }

            if (minCapacity.HasValue)
                query = query.Where(a => a.Capacity >= minCapacity.Value);

            return query.OrderBy(a => a.AmenityName).ToPagedResultAsync(paging);
        }
    }
}
