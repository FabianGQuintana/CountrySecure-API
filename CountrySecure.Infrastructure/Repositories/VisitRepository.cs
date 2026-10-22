using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories;

public class VisitRepository : GenericRepository<Visit>, IVisitRepository
{
    public VisitRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

    public Task<Visit?> GetByDniIncludingDeletedAsync(int dni)
        => Set.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.DniVisit == dni);

    public Task<Visit?> GetWithPermitsAsync(Guid visitId)
        => Set.Include(v => v.EntryPermissions).FirstOrDefaultAsync(v => v.Id == visitId);

    public Task<PagedResult<Visit>> GetPagedAsync(PageQuery paging, int? dni, string? search, bool includeDeleted)
    {
        var query = Set.AsNoTracking().IncludeDeleted(includeDeleted);

        if (dni.HasValue)
            query = query.Where(v => v.DniVisit == dni.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(v => (v.NameVisit + " " + v.LastNameVisit).ToLower().Contains(term)
                                     || v.DniVisit.ToString().Contains(term));
        }

        return query.OrderBy(v => v.LastNameVisit).ThenBy(v => v.NameVisit).ToPagedResultAsync(paging);
    }
}
