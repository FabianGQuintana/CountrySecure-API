using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories;

public class RequestRepository : GenericRepository<Request>, IRequestRepository
{
    public RequestRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

    public Task<Request?> GetWithDetailsAsync(Guid id, bool includeDeleted = false)
        => Set.IncludeDeleted(includeDeleted)
              .Include(r => r.User)
              .Include(r => r.Order)
              .FirstOrDefaultAsync(r => r.Id == id);

    public Task<PagedResult<Request>> GetPagedAsync(PageQuery paging, RequestStatus? status, Guid? userId, bool includeDeleted)
    {
        var query = Set.AsNoTracking()
                       .IncludeDeleted(includeDeleted)
                       .Include(r => r.User)
                       .Include(r => r.Order)
                       .AsQueryable();

        if (status.HasValue) query = query.Where(r => r.RequestStatus == status.Value);
        if (userId.HasValue) query = query.Where(r => r.UserId == userId.Value);

        return query.OrderByDescending(r => r.CreatedAt).ToPagedResultAsync(paging);
    }

    public Task<int> CountByStatusAsync(RequestStatus status)
        => Set.CountAsync(r => r.RequestStatus == status);
}
