using System;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Repositories;

public interface IPropertyRepository : IGenericRepository<Property>
{
	
	Task<Property?> GetPropertyByAdressAsync(string street, int numberProperty);

	Task<IEnumerable<Property>> GetPropertyByIdUserAsync(int idUser);

	Task<IEnumerable<Property>> GetPropertyByIdLotAsync(int idLot);

    Task<IEnumerable<Property>> GetPropertiesByStatusAsync(PropertyStatus status,int pageNumber,int pageSize);

}
