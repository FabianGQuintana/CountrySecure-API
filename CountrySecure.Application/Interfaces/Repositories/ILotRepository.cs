using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface ILotRepository : IGenericRepository<Lot>
    {
        /// <summary>Busca por nombre incluyendo lotes dados de baja (el nombre es único en toda la tabla).</summary>
        Task<Lot?> GetByNameIncludingDeletedAsync(string lotName);

        Task<IReadOnlyList<string>> GetDistinctBlockNamesAsync();

        Task<PagedResult<Lot>> GetPagedAsync(PageQuery paging, LotStatus? state, string? search, bool includeDeleted);
    }
}
