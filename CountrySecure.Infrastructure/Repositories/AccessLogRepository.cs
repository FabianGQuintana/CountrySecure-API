using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class AccessLogRepository : GenericRepository<AccessLog>, IAccessLogRepository
    {
        public AccessLogRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        public Task<PagedResult<AccessLog>> GetPagedAsync(
            PageQuery paging, Guid? permissionId, ScanOutcome? outcome, DateTime? from, DateTime? to)
        {
            // La auditoría conserva los nombres aunque el guardia o el visitante se hayan dado de baja
            var query = Set.AsNoTracking()
                           .IgnoreQueryFilters()
                           .Include(l => l.Guard)
                           .Include(l => l.Permission).ThenInclude(p => p!.Visit)
                           .AsQueryable();

            if (permissionId.HasValue) query = query.Where(l => l.PermissionId == permissionId.Value);
            if (outcome.HasValue) query = query.Where(l => l.Outcome == outcome.Value);
            if (from.HasValue) query = query.Where(l => l.ScannedAt >= from.Value);
            if (to.HasValue) query = query.Where(l => l.ScannedAt <= to.Value);

            return query.OrderByDescending(l => l.ScannedAt).ToPagedResultAsync(paging);
        }
    }
}
