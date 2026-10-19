using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IVisitRepository : IGenericRepository<Visit>
{
    /// <summary>Busca por DNI incluyendo visitas dadas de baja (el DNI es único en toda la tabla).</summary>
    Task<Visit?> GetByDniIncludingDeletedAsync(int dni);

    Task<Visit?> GetWithPermitsAsync(Guid visitId);

    Task<PagedResult<Visit>> GetPagedAsync(PageQuery paging, int? dni, string? search, bool includeDeleted);
}
