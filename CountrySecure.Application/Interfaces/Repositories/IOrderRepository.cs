using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<PagedResult<Order>> GetPagedAsync(PageQuery paging, OrderCategory? type, string? search, bool includeDeleted);

    /// <summary>Los servicios con más solicitudes asociadas.</summary>
    Task<IReadOnlyList<Order>> GetMostRequestedAsync(int take);
}
