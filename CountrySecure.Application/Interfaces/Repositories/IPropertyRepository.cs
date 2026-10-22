using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IPropertyRepository : IGenericRepository<Property>
{
    /// <summary>Busca por dirección incluyendo propiedades dadas de baja (la dirección es única en toda la tabla).</summary>
    Task<Property?> GetByAddressIncludingDeletedAsync(string street, int propertyNumber);

    /// <summary>Propiedad con Lot y User cargados.</summary>
    Task<Property?> GetByIdWithIncludesAsync(Guid id, bool includeDeleted = false);

    Task<PagedResult<Property>> GetPagedAsync(PageQuery paging, PropertyStatus? status, Guid? lotId, Guid? ownerId, string? search, bool includeDeleted);
}
