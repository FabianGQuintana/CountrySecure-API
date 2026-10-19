using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IAccessLogRepository : IGenericRepository<AccessLog>
    {
        Task<PagedResult<AccessLog>> GetPagedAsync(PageQuery paging, Guid? permissionId, ScanOutcome? outcome, DateTime? from, DateTime? to);
    }
}
