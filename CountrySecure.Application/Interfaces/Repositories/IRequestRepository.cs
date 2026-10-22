using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IRequestRepository : IGenericRepository<Request>
{
    /// <summary>Solicitud con User y Order cargados.</summary>
    Task<Request?> GetWithDetailsAsync(Guid id, bool includeDeleted = false);

    Task<PagedResult<Request>> GetPagedAsync(PageQuery paging, RequestStatus? status, Guid? userId, bool includeDeleted);

    Task<int> CountByStatusAsync(RequestStatus status);
}
