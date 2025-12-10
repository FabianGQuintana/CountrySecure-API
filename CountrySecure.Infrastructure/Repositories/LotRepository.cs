
using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Interfaces;

namespace CountrySecure.Infrastructure.Repositories
{
    public class LotRepository : GenericRepository<Lot>, ILotRepository
    {
        private readonly CountrySecureDbContext _dbContext;

        public LotRepository(CountrySecureDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<Lot?> GetLotByNameLotAsync(string nameLot)
        {
            return await _dbContext.Lots
                .Where(l => l.LotName == nameLot)
                .FirstOrDefaultAsync();

        }

        public async Task<Lot?> GetLotByNameBlockAsync(string blockName)
        {
            return await _dbContext.Lots
                .Where(l => l.BlockName == blockName)
                .FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<string>> GetDistinctBlockNamesAsync()
        {
            return await _dbContext.Lots
                                   .Select(l => l.BlockName)
                                   .Distinct()
                                   .ToListAsync();
        }


        public async Task<IEnumerable<Lot>> GetLotsByStatusAsync(LotStatus status, int pageNumber, int pageSize)
        {
            return await _dbContext.Lots
                                   .Where(l => l.Status == status.ToString())
                                   .Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
        }

        public async Task<Lot?> SoftDeleteLotAsync(Guid lotId)
        {
            var lot = await _dbContext.Lots.FirstOrDefaultAsync(l => l.Id == lotId);

            if (lot == null)
                return null;

            // Si está activo → desactivar
            if (lot.DeletedAt == null)
            {
                lot.DeletedAt = DateTime.UtcNow;
                lot.Status = "Inactive";
                lot.LotState = LotStatus.Inactive;
            }
            else
            {
                // Si está inactivo → activar
                lot.DeletedAt = null;
                lot.Status = "Active";
                lot.LotState = LotStatus.Available;
                
            }

            lot.UpdatedAt = DateTime.UtcNow;

            return lot;
        }


    }
}
