using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Visits;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IVisitService
    {
        Task<VisitResponseDto> CreateAsync(CreateVisitDto dto, Guid currentUserId);
        Task<VisitResponseDto> UpdateAsync(Guid id, UpdateVisitDto dto, Guid currentUserId);
        Task<VisitResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<VisitResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<VisitResponseDto> GetByIdAsync(Guid id);
        Task<VisitWithPermitsDto> GetWithPermitsAsync(Guid id);
        Task<IReadOnlyList<VisitEntryPermissionDto>> GetValidPermitsAsync(Guid id);
        Task<PagedResult<VisitResponseDto>> GetPagedAsync(PageQuery paging, int? dni, string? search, bool includeInactive);
    }
}
