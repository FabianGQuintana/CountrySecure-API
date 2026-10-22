using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface ITurnRepository : IGenericRepository<Turn>
    {
        /// <summary>Turno con User y Amenity cargados.</summary>
        Task<Turn?> GetByIdWithIncludesAsync(Guid id, bool includeDeleted = false);

        Task<PagedResult<Turn>> GetPagedAsync(
            PageQuery paging, Guid? userId, Guid? amenityId, DateTime? from, DateTime? to, TurnStatus? status, bool includeDeleted);

        /// <summary>
        /// Turnos activos (no cancelados ni dados de baja) del amenity que se solapan con [start, end).
        /// </summary>
        Task<List<Turn>> GetActiveOverlappingTurnsAsync(Guid amenityId, DateTime start, DateTime end, Guid? excludeTurnId = null);
    }
}
