using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CountrySecure.Application.Interfaces.Repositories;

public interface IRequestRepository: IGenericRepository<Request>
{
    Task<IEnumerable<Request>> GetByStatusAsync(RequestStatus status, int numberPage, int pageSize);
    Task<IEnumerable<Request>> GetByUserIdAsync(Guid userId);
    Task<int> CountByStatusAsync(RequestStatus status);
    Task<Request?> GetRequestWithDetailsAsync(Guid id);
    Task<IEnumerable<Request>> GetAllRequestsWithDetailsAsync(int numberPage, int pageSize);
    Task<Request?> ToggleActiveAsync(Guid id);
}
