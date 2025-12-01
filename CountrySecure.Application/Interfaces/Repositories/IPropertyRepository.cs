using System;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IPropertyRepository : IGenericRepository<Property>
{

	Task<Property?> GetPropertyByAdressAsync(string street, int PropertyNumber);

	Task<IEnumerable<Property>> GetPropertyByUserIdAsync(Guid UserId);

	Task<IEnumerable<Property>> GetPropertyByLotIdAsync(Guid LotId);

	Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize);

}
