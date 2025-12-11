using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IEntryPermissionRepository : IGenericRepository<EntryPermission>
    {
        //Obtener permiso de entrada por valor de código QR.
        Task<EntryPermission?> GetEntryPermissionByQrCodeValueAsync(string qrCodeValue);

        //Obtener todos los permisos de entrada asociados a un usuario,visita,servicio específico.
        Task<IEnumerable<EntryPermission>> GetEntryPermissionsByUserIdAsync(Guid userId);

        Task<IEnumerable<EntryPermission>> GetEntryPermissionsByVisitIdAsync(Guid visitId);

        Task<IEnumerable<EntryPermission>> GetEntryPermissionsByServiceIdAsync(Guid serviceId);

        //Obtener los permisos de entrada por tipo de permiso (visita, mantenimiento).
        Task<IEnumerable<EntryPermission>> GetEntryPermissionsByTypeAsync(PermissionType permissionType, int pageNumber, int pageSize);

        //Obtener permisos de entrada por estado con paginación.
        Task<IEnumerable<EntryPermission>> GetEntryPermissionsStatusAsync(string status, int pageNumber, int pageSize);

        //Obtener permiso de entrada por valor de código QR.
        Task<EntryPermission?> GetByQrCodeValueAsync(string qrCodeValue);

        Task<EntryPermission?> GetEntryPermissionWithDetailsAsync(Guid id);

        Task<EntryPermission?> GetByIdWithIncludesAsync(Guid id);

        // Modificar este método para que cargue las inclusiones necesarias para el mapeo
        Task<IEnumerable<EntryPermission>> GetAllWithIncludesAsync(int pageNumber, int pageSize);
    }
}
