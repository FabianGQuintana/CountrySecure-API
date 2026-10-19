using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IEntryPermissionService
    {
        Task<EntryPermissionResponseDto> CreateAsync(CreateEntryPermissionDto dto, Guid currentUserId);
        Task<EntryPermissionResponseDto> UpdateAsync(Guid id, UpdateEntryPermissionDto dto, Guid currentUserId);

        /// <summary>Cancela el permiso (baja lógica). Solo si todavía no se usó.</summary>
        Task<EntryPermissionResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<EntryPermissionResponseDto> RestoreAsync(Guid id, Guid currentUserId);

        /// <summary>Un residente solo puede ver los propios; Admin y Seguridad cualquiera.</summary>
        Task<EntryPermissionResponseDto> GetByIdAsync(Guid id);

        Task<PagedResult<EntryPermissionResponseDto>> GetPagedAsync(
            PageQuery paging,
            PermissionType? type,
            PermissionStatus? status,
            Guid? userId,
            Guid? visitId,
            Guid? orderId,
            string? search,
            bool includeInactive);

        // --- Portería ---
        Task<GateCheckResponseDto> ValidateQrCodeAsync(string qrCodeValue);
        /// <summary>Permisos del día (según la zona horaria del servidor): pendientes de ingreso o con la visita adentro.</summary>
        Task<PagedResult<EntryPermissionResponseDto>> GetTodayAsync(PageQuery paging);
        Task<PagedResult<EntryPermissionResponseDto>> GetHistoryAsync(PageQuery paging, string? search, PermissionType? type);
        Task<EntryPermissionResponseDto> RegisterCheckInAsync(Guid permissionId, Guid guardId);
        Task<EntryPermissionResponseDto> RegisterCheckOutAsync(Guid permissionId, Guid guardId);
    }
}
