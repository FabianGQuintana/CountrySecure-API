using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Properties;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IPropertyService
    {


        Task<PropertyDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto);

        Task<PropertyDto?> GetPropertyByIdAsync(Guid propertyId);

        Task<IEnumerable<PropertyDto>> GetPropertiesByOwnerId(Guid ownerId);

        Task UpdatePropertyAsync(Property updateProperty);

        Task<bool> SoftDeletePropertyAsync(Guid propertyId);

        Task<IEnumerable<PropertyDto>> GetAllPropertiesAsync(int pageNumber, int pageSize);

        Task <IEnumerable<PropertyDto>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize);

    }
}
