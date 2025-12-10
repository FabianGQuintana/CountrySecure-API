using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
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
            .Include(v => v.EntryPermissions)
            .FirstOrDefaultAsync(v => v.Id == visitId);
    }

    public async Task<IEnumerable<EntryPermission>> GetPermitsByVisitIdAsync(Guid visitId)
     {
         return await _dbContext.EntryPermissions
             .Where(ep => ep.VisitId == visitId)
             .ToListAsync();
     }

     public async Task<EntryPermission?> GetValidPermitByVisitIdAsync(Guid visitId)
     {
         return await _dbContext.EntryPermissions
             .Where(ep => ep.VisitId == visitId &&
                          ep.Status == PermissionStatus.Pending &&
                         ep.ValidFrom.Date >= DateTime.UtcNow.Date)
             .FirstOrDefaultAsync();
     }

    public async Task<IEnumerable<Visit>> GetAllWithoutPaginationAsync()
    {
        return await _dbContext.Visits
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();
    }
    public async Task<Visit?> GetByIdWithoutFiltersAsync(Guid id)
    {
        return await _dbContext.Visits
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Visit?> SoftDeleteVisitAsync(Guid visitId)
    {
        var visit = await _dbContext.Visits
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null )
            return null;

        visit.Status = "Inactive";
        visit.DeletedAt = DateTime.UtcNow;
        visit.UpdatedAt = DateTime.UtcNow;

        return visit; // EF ya lo trackea
    }


}
