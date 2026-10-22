using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

    public Task<PagedResult<Order>> GetPagedAsync(PageQuery paging, OrderCategory? type, string? search, bool includeDeleted)
    {
        var query = Set.AsNoTracking().IncludeDeleted(includeDeleted);

        if (type.HasValue)
            query = query.Where(o => o.Category == type.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(o => o.SupplierName.ToLower().Contains(term) || o.Description.ToLower().Contains(term));
        }

        return query.OrderBy(o => o.SupplierName).ThenBy(o => o.CreatedAt).ToPagedResultAsync(paging);
    }

    public async Task<IReadOnlyList<Order>> GetMostRequestedAsync(int take)
        => await Set.AsNoTracking()
                    .OrderByDescending(o => o.Requests!.Count)
                    .Take(take)
                    .ToListAsync();
}
