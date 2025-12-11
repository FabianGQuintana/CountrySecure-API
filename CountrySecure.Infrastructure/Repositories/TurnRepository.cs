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
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByUserId(Guid userId)
        {
            return await _dbContext.Turns
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.Turns
                .Where(t => t.StartTime >= startDate && t.EndTime <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Turn>> GetTurnsByStatus(TurnStatus status)
        {
            return await _dbContext.Turns
                .Where(t => t.TurnStatus == status)
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


    }
}
