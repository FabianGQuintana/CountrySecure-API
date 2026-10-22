using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface IAmenityRepository : IGenericRepository<Amenity>
    {
        /// <summary>Amenity activo con ese nombre (para validar unicidad).</summary>
        Task<Amenity?> GetByNameAsync(string amenityName);

        Task<PagedResult<Amenity>> GetPagedAsync(PageQuery paging, string? search, int? minCapacity, bool includeDeleted);
    }
}
