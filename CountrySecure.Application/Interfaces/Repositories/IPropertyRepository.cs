using System;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IPropertyRepository : IGenericRepository<Property>
{
	
	Task<Property?> GetPropertyByAdressAsync(string street, int numberProperty);

	Task<IEnumerable<Property>> GetPropertyByIdUserAsync(Guid userId);

    Task<IEnumerable<Property>> GetPropertiesByLotIdAsync(Guid lotId);

    Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status,int pageNumber,int pageSize);

    Task<Property?> GetByIdWithIncludesAsync(Guid id);
    Task<Property?> SoftDeleteAsync(Guid id);
}
