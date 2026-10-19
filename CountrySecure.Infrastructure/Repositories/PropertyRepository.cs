using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories;

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    public PropertyRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

    public Task<Property?> GetByAddressIncludingDeletedAsync(string street, int propertyNumber)
        => Set.IgnoreQueryFilters()
              .FirstOrDefaultAsync(p => p.Street == street && p.PropertyNumber == propertyNumber);

    public Task<Property?> GetByIdWithIncludesAsync(Guid id, bool includeDeleted = false)
        => Set.IncludeDeleted(includeDeleted)
              .Include(p => p.Lot)
              .Include(p => p.User)
              .FirstOrDefaultAsync(p => p.Id == id);

    public Task<PagedResult<Property>> GetPagedAsync(
        PageQuery paging, PropertyStatus? status, Guid? lotId, Guid? ownerId, string? search, bool includeDeleted)
    {
        var query = Set.AsNoTracking()
                       .IncludeDeleted(includeDeleted)
                       .Include(p => p.Lot)
                       .Include(p => p.User)
                       .AsQueryable();

        if (status.HasValue) query = query.Where(p => p.PropertyStatus == status.Value);
        if (lotId.HasValue) query = query.Where(p => p.LotId == lotId.Value);
        if (ownerId.HasValue) query = query.Where(p => p.UserId == ownerId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Street.ToLower().Contains(term)
                                     || (p.Lot != null && p.Lot.LotName.ToLower().Contains(term))
                                     || (p.User != null && (p.User.Name + " " + p.User.Lastname).ToLower().Contains(term)));
        }

        return query.OrderBy(p => p.Street).ThenBy(p => p.PropertyNumber).ToPagedResultAsync(paging);
    }
}
