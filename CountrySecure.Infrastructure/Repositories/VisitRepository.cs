using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories;

public class VisitRepository : GenericRepository<Visit>, IVisitRepository
{
    private readonly CountrySecureDbContext _dbContext;

    public VisitRepository(CountrySecureDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Visit>> GetVisitsByDniAsync(int dniVisit)
    {
        return await _dbContext.Visits
            .Where(v => v.DniVisit == dniVisit)
            .ToListAsync();
    }

    public async Task<Visit?> GetVisitWithPermitsAsync(Guid visitId)
    {
        return await _dbContext.Visits
            .Include(v => v.EntryPermits)
            .FirstOrDefaultAsync(v => v.Id == visitId);
    }

    public async Task<IEnumerable<EntryPermit>> GetPermitsByVisitIdAsync(Guid visitId)
    {
        return await _dbContext.EntryPermit
            .Where(ep => ep.VisitId == visitId)
            .ToListAsync();
    }

    public async Task<EntryPermit?> GetValidPermitByVisitIdAsync(Guid visitId)
    {
        return await _dbContext.EntryPermit
            .Where(ep => ep.VisitId == visitId &&
                         ep.Status == EntryPermitState.Valid &&
                         ep.FechaVisita.Date >= DateTime.UtcNow.Date)
            .FirstOrDefaultAsync();
    }
}
