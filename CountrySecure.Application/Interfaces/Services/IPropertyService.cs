using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Properties;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IPropertyService
    {


        Task<PropertyResponseDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto);

        Task<PropertyResponseDto?> GetPropertyByIdAsync(Guid propertyId);

        Task<IEnumerable<PropertyResponseDto>> GetPropertiesByOwnerId(Guid ownerId);

        Task<IEnumerable<PropertyResponseDto>> GetPropertiesByLotIdAsync(Guid lotId);

        Task UpdatePropertyAsync(UpdatePropertyDto updateProperty,Guid currentId);

        Task<bool> SoftDeletePropertyAsync(Guid propertyId, Guid currentUserId);

        Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync(int pageNumber, int pageSize);

        Task <IEnumerable<PropertyResponseDto>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize);
        
    }
}
