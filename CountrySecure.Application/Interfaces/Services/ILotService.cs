using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Lots;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ILotService
    {
        Task<LotResponseDto> AddNewLotAsync(CreateLotDto newLotDto,Guid currentUserId);

        Task<LotResponseDto?> GetLotByIdAsync(Guid lotId);

        Task<IEnumerable<LotResponseDto>> GetAllLotsAsync(int pageNumber, int pageSize);

        Task<IEnumerable<string>> GetAllBlockNamesAsync();

        Task<LotResponseDto> UpdateAsync(UpdateLotDto updateLot, Guid lotId, Guid currentUserId);

        Task<LotResponseDto?> SoftDeleteToggleAsync(Guid lotId, Guid currentUserId);

        Task<IEnumerable<LotResponseDto>> GetLotsByStatusAsync(LotStatus status, int pageNumber, int pageSize);
    }
}
