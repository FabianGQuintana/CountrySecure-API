using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface ITurnService
    {
        Task<TurnResponseDto> CreateAsync(CreateTurnDto dto, Guid currentUserId);
        Task<TurnResponseDto> UpdateAsync(Guid id, UpdateTurnDto dto, Guid currentUserId);

        /// <summary>Cancela la reserva (baja lógica). Un residente solo puede cancelar las propias.</summary>
        Task<TurnResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<TurnResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<TurnResponseDto> GetByIdAsync(Guid id);

        /// <summary>Quien no es Admin solo ve sus propias reservas.</summary>
        Task<PagedResult<TurnResponseDto>> GetPagedAsync(
            PageQuery paging, Guid? userId, Guid? amenityId, DateTime? from, DateTime? to, TurnStatus? status, bool onlyMine, bool includeInactive);
    }
}
