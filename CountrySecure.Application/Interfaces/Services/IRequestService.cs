using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Request;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IRequestService
    {
        Task<RequestResponseDto> CreateAsync(CreateRequestDto dto, Guid currentUserId);
        Task<RequestResponseDto> UpdateAsync(Guid id, UpdateRequestDto dto, Guid currentUserId);

        /// <summary>Cancela la solicitud (baja lógica). Un residente solo puede cancelar las propias.</summary>
        Task<RequestResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<RequestResponseDto> RestoreAsync(Guid id, Guid currentUserId);

        /// <summary>Un residente solo puede ver las propias; un Admin cualquiera.</summary>
        Task<RequestResponseDto> GetByIdAsync(Guid id);

        /// <summary>Si <paramref name="onlyMine"/> es true (o el usuario no es Admin) se limita a las del usuario actual.</summary>
        Task<PagedResult<RequestResponseDto>> GetPagedAsync(PageQuery paging, RequestStatus? status, bool onlyMine, bool includeInactive);

        Task<int> CountByStatusAsync(RequestStatus status);
    }
}
