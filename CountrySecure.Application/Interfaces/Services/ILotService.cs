using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ILotService
    {
        Task<LotResponseDto> CreateAsync(CreateLotDto dto, Guid currentUserId);
        Task<LotResponseDto> UpdateAsync(Guid id, UpdateLotDto dto, Guid currentUserId);
        Task<LotResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<LotResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<LotResponseDto> GetByIdAsync(Guid id);
        Task<IReadOnlyList<string>> GetBlockNamesAsync();
        Task<PagedResult<LotResponseDto>> GetPagedAsync(PageQuery paging, LotStatus? state, string? search, bool includeInactive);
    }
}
