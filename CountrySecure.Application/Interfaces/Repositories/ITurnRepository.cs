using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;


namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface ITurnRepository : IGenericRepository<Turn>
    {
        Task<IEnumerable<Turn>> GetTurnsByAmenityId(Guid amenityId);

        Task<IEnumerable<Turn>> GetTurnsByUserId(Guid userId);

        Task<IEnumerable<Turn>> GetTurnsByDateRange(DateTime startDate, DateTime endDate);

        Task<IEnumerable<Turn>> GetTurnsByStatus(TurnStatus status);

        Task<Turn?> GetByIdWithIncludesAsync(Guid id);

        Task<IEnumerable<Turn>> GetAllWithIncludesAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Turnos activos (no cancelados ni dados de baja) del amenity que se solapan con [start, end).
        /// </summary>
        Task<List<Turn>> GetActiveOverlappingTurnsAsync(Guid amenityId, DateTime start, DateTime end, Guid? excludeTurnId = null);


    }
}
