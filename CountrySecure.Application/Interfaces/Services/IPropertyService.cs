using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IPropertyService
    {
        Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid currentUserId);
        Task<PropertyResponseDto> UpdateAsync(Guid id, UpdatePropertyDto dto, Guid currentUserId);
        Task<PropertyResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<PropertyResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<PropertyResponseDto> GetByIdAsync(Guid id);
        Task<PagedResult<PropertyResponseDto>> GetPagedAsync(
            PageQuery paging, PropertyStatus? status, Guid? lotId, Guid? ownerId, string? search, bool includeInactive);
    }
}
