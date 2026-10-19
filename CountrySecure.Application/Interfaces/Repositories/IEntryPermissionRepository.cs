using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IEntryPermissionRepository : IGenericRepository<EntryPermission>
    {
        /// <summary>Permiso con residente, visitante, orden y guardias cargados.</summary>
        Task<EntryPermission?> GetWithDetailsAsync(Guid id, bool includeDeleted = false);

        /// <summary>Busca por QR. Con <paramref name="includeDeleted"/> encuentra también los permisos cancelados (para informar al guardia).</summary>
        /// <summary>Lotes/casas del residente, listos para mostrar ("Calle 123 · Lote A-1").</summary>
        Task<List<string>> GetDestinationsAsync(Guid userId);

        Task<EntryPermission?> GetByQrCodeAsync(string qrCodeValue, bool includeDeleted = false);

        Task<PagedResult<EntryPermission>> GetPagedAsync(
            PageQuery paging,
            PermissionType? type,
            PermissionStatus? status,
            Guid? userId,
            Guid? visitId,
            Guid? orderId,
            string? search,
            bool includeDeleted);

        /// <summary>Permisos del día (pendientes de ingreso o con la visita adentro), a partir de <paramref name="dayStartUtc"/>.</summary>
        Task<PagedResult<EntryPermission>> GetTodayAsync(PageQuery paging, DateTime dayStartUtc);

        /// <summary>Historial de ingresos efectivos (con EntryTime), más recientes primero.</summary>
        Task<PagedResult<EntryPermission>> GetHistoryAsync(PageQuery paging, string? search, PermissionType? type);
    }
}
