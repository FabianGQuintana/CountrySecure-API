using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class LotRepository : GenericRepository<Lot>, ILotRepository
    {
        public LotRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        public Task<Lot?> GetByNameIncludingDeletedAsync(string lotName)
            => Set.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LotName == lotName);

        public async Task<IReadOnlyList<string>> GetDistinctBlockNamesAsync()
            => await Set.AsNoTracking().Select(l => l.BlockName).Distinct().OrderBy(b => b).ToListAsync();

        public Task<PagedResult<Lot>> GetPagedAsync(PageQuery paging, LotStatus? state, string? search, bool includeDeleted)
        {
            var query = Set.AsNoTracking().IncludeDeleted(includeDeleted);

            if (state.HasValue)
                query = query.Where(l => l.LotState == state.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l => l.LotName.ToLower().Contains(term) || l.BlockName.ToLower().Contains(term));
            }

            return query.OrderBy(l => l.BlockName).ThenBy(l => l.LotName).ToPagedResultAsync(paging);
        }
    }
}
