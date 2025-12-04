using CountrySecure.Application.DTOs.Amenity;
using CountrySecure.Domain.Entities; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IAmenityService
    {
        Task<AmenityResponseDto> CreateAmenityAsync(AmenityCreateDto dto);
        Task<AmenityResponseDto> AmenityUpdateAsync(Guid id, AmenityUpdateDto dto);
        Task<bool> DeleteAmenityAsync(Guid id);
        Task<AmenityResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AmenityResponseDto>> GetAllAsync(int page, int size);
    }
}