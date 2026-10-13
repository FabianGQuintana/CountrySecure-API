using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace CountrySecure.Infrastructure.Repositories
{
    public class TurnRepository : GenericRepository<Turn>,ITurnRepository
    {
        private readonly CountrySecureDbContext _dbContext;

        public TurnRepository(CountrySecureDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Turn>> GetTurnsByAmenityId(Guid amenityId)
        {
            return await _dbContext.Turns
                .Where(t => t.AmenityId == amenityId)
                .Include(t => t.User)
                .Include(t => t.Amenity)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByUserId(Guid userId)
        {
            return await _dbContext.Turns
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .Include(t => t.Amenity)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Turns
                .Where(t => t.StartTime >= startDate && t.EndTime <= endDate)
                .Include(t => t.User)
                .Include(t => t.Amenity)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByStatus(TurnStatus status)
        {
            return await _dbContext.Turns
                .Where(t => t.TurnStatus == status)
                .Include(t => t.User)
                .Include(t => t.Amenity)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }


        public async Task<Turn?> GetByIdWithIncludesAsync(Guid id)
        {
            return await _dbContext.Turns
                .Include(t => t.User)     // Para Resident/Owner
                .Include(t => t.Amenity)  // Para la Amenity
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        public async Task<IEnumerable<Turn>> GetAllWithIncludesAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.Set<Turn>()
                .Include(t => t.User)     // Carga la relación con User
                .Include(t => t.Amenity)  // Carga la relación con Amenity
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Turn>> GetActiveOverlappingTurnsAsync(Guid amenityId, DateTime start, DateTime end, Guid? excludeTurnId = null)
        {
            // Dos intervalos [a,b) y [c,d) se solapan si a < d && c < b
            return await _dbContext.Turns
                .AsNoTracking()
                .Where(t => t.AmenityId == amenityId
                            && t.DeletedAt == null
                            && t.TurnStatus != TurnStatus.Cancelled
                            && t.StartTime < end
                            && start < t.EndTime
                            && (excludeTurnId == null || t.Id != excludeTurnId))
                .ToListAsync();
        }


    }
}
