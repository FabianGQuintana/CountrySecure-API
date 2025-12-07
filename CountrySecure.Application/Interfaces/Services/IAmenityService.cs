using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Domain.Entities; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IAmenityService
    {
       
        Task<AmenityResponseDto> AmenityCreateAsync(AmenityCreateDto dto, Guid createdById); 
        Task<AmenityResponseDto> AmenityUpdateAsync(Guid id, AmenityUpdateDto dto, Guid modifiedById); 
        Task<bool> DeleteAmenityAsync(Guid id, Guid currentUserId); 
        Task<AmenityResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AmenityResponseDto>> GetAllAsync(int page, int size);

        Task<AmenityResponseDto> GetAmenityByNameAsync(string amenityName);

        Task<IEnumerable<AmenityResponseDto>> GetAllAmenitiesWithTurnsAsync(int pageNumber, int pageSize);

        Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByCapacityAsync(int minimumCapacity);

        Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByStatusAsync(string status);

    }
}