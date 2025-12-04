using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Interfaces;

namespace CountrySecure.Infrastructure.Repositories
{
    public class AmenityRepository : GenericRepository<Amenity>, IAmenityRepository
    {
        private readonly CountrySecureDbContext _dbContext;
        public AmenityRepository(CountrySecureDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Amenity?> GetAmenityByNameAsync(string amenityName)
        {
            return await _dbContext.Amenities
                .Where(a => a.AmenityName == amenityName)
                .FirstOrDefaultAsync();
        }

        public async Task<Amenity?> GetAmenityWithTurnosAsync(Guid amenityId)
        {
            return await _dbContext.Amenities
                .Include(a => a.Turns)
                .FirstOrDefaultAsync(a => a.Id == amenityId);
        }

        public async Task<IEnumerable<Amenity>> GetAvailableAmenitiesAsync()
        {
            return await _dbContext.Amenities
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Amenity>> GetAllAmenitiesWithTurnosAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.Amenities
                                   .Include(a => a.Turns)
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
        }

        public async Task<bool> IsAmenityAvailableForBookingAsync(Guid amenityId, DateTime startTime, DateTime endTime)
        {
            var amenity = await _dbContext.Amenities
                                          .Include(a => a.Turns)
                                          .FirstOrDefaultAsync(a => a.Id == amenityId);
            if (amenity == null)
            {
                return false;
            }
            foreach (var turno in amenity.Turns)
            {
                if (startTime < turno.EndTime && endTime > turno.StartTime)
                {
                    return false; 
                }
            }
            return true; 
        }
    }
}
