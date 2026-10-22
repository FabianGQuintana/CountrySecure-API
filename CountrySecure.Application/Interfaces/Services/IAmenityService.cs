using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Application.DTOs.Common;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IAmenityService
    {
        Task<AmenityResponseDto> CreateAsync(AmenityCreateDto dto, Guid currentUserId);
        Task<AmenityResponseDto> UpdateAsync(Guid id, AmenityUpdateDto dto, Guid currentUserId);
        Task<AmenityResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<AmenityResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<AmenityResponseDto> GetByIdAsync(Guid id);
        Task<PagedResult<AmenityResponseDto>> GetPagedAsync(PageQuery paging, string? search, int? minCapacity, bool includeInactive);
    }
}
