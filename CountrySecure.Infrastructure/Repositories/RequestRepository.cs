using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Infrastructure.Repositories;

public class RequestRepository : GenericRepository<Request>, IRequestRepository
{
    private readonly CountrySecureDbContext _dbContext;

    public RequestRepository(CountrySecureDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountByStatusAsync(RequestStatus status)
    {
        return await _dbContext.Requests
                               .Where(r => (int)r.RequestStatus == (int)status)  
                               .CountAsync();
    }


    public async Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, int numberPage, int pageSize)
    {
        return await _dbContext.Requests
                               .Where(r => (int)r.RequestStatus == (int)status)  
                               .OrderBy(r => r.Id)
                               .Skip((numberPage - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();
    }


    public async Task<IEnumerable<Request>> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.Requests
                               .Where(r => r.IdUser == userId)
                               .ToListAsync();
    }
}
