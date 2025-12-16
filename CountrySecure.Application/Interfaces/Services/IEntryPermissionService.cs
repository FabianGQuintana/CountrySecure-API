
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.Interfaces.Services
{
    public interface IEntryPermissionService
    {

        //Creación de nuevo permiso de entrada
        Task<EntryPermissionResponseDto> AddNewEntryPermissionAsync(CreateEntryPermissionDto dto, Guid currentUserId);

        //Obtención por Id
        Task<EntryPermissionResponseDto?> GetEntryPermissionByIdAsync(Guid entryPermissionId);

        //Obtención por UserId
        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByUserIdAsync(Guid userId);

        //Obtención por ServiceId
        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByServiceIdAsync(Guid serviceId);

        //Obtención por VisitId
        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByVisitIdAsync(Guid visitId);

        //Obtención de todos los permisos de entrada con paginación
        Task<IEnumerable<EntryPermissionResponseDto>> GetAllEntryPermissionsAsync(int pageNumber, int pageSize);

        //Actualización de permiso de entrada
        Task<EntryPermissionResponseDto?> UpdateEntryPermissionAsync(UpdateEntryPermissionDto dto, Guid entryPermissionId,Guid currentUserId);

        //Eliminación lógica de permiso de entrada
        Task<EntryPermissionResponseDto?> SoftDeleteEntryPermissionAsync(Guid entryPermissionId, Guid currentUserId);

        //Obtención de permisos por tipo de permiso.
        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByTypeAsync(PermissionType permissionType, int pageNumber, int pageSize);

        //Obtención de permisos por estado (1-ALTA/0-BAJA).
        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryPermissionsByStatusAsync(string status, int pageNumber, int pageSize);

        // Manejo de QRCode.
        Task<GateCheckResponseDto> ValidateQrCodeAsync(string qrCodeValue);

        Task<PaginatedResult<EntryPermissionResponseDto>> GetActivePermissionsForDateAsync(DateTime date, int pageNumber, int pageSize);

        Task<EntryPermissionResponseDto> RegisterCheckInAsync(Guid permissionId, Guid currentUserId);

        Task<EntryPermissionResponseDto> RegisterCheckOutAsync(Guid permissionId, Guid currentUserId);

        Task<IEnumerable<EntryPermissionResponseDto>> GetEntryLogsAsync(int pageNumber, int pageSize, string? search);

    }
}
