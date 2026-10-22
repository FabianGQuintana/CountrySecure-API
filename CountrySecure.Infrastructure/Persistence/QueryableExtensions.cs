using CountrySecure.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Persistence
{
    public static class QueryableExtensions
    {
        /// <summary>Pagina EN LA BASE (COUNT + OFFSET/LIMIT) y devuelve el formato único <see cref="PagedResult{T}"/>.</summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, PageQuery paging, CancellationToken ct = default)
        {
            var total = await query.CountAsync(ct);
            var items = await query.Skip(paging.Skip).Take(paging.NormalizedPageSize).ToListAsync(ct);
            return new PagedResult<T>(items, paging.NormalizedPage, paging.NormalizedPageSize, total);
        }

        /// <summary>Si <paramref name="includeDeleted"/> es true, ignora el filtro global de baja lógica.</summary>
        public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query, bool includeDeleted) where T : class
            => includeDeleted ? query.IgnoreQueryFilters() : query;
    }
}
