using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountrySecure.Application.DTOs.Amenity;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Mappers; 

namespace CountrySecure.Application.Services
{
    public class AmenityService : IAmenityService
    {
        // Dependencias (Inyección de Interfaces)
        private readonly IAmenityRepository _amenityRepository;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IAmenityRepository amenityRepository, ITurnoRepository turnoRepository, IUnitOfWork unitOfWork)
        {
            _amenityRepository = amenityRepository;
            _turnoRepository = turnoRepository;
            _unitOfWork = unitOfWork;
        }


        
            public async Task<AmenityResponseDto> AmenityCreateAsync(AmenityCreateDto dto, Guid createdById)
            {
            var existingAmenity = await _amenityRepository.GetAmenityByNameAsync(dto.AmenityName);
            if (existingAmenity != null)
            {
                throw new InvalidOperationException("the amenity does not exist");
            }


            var amenity = new Amenity
            {
                AmenityName = dto.AmenityName,
                Description = dto.Description,
                Schedules = dto.Schedules,
                Capacity = dto.Capacity,

                CreatedBy = createdById.ToString(),
            };


            await _amenityRepository.AddAsync(amenity);
            await _unitOfWork.SaveChangesAsync();


            return AmenityMapper.ToAmenityResponseDto(amenity);
        }

        public async Task<AmenityResponseDto> AmenityUpdateAsync(Guid id, AmenityUpdateDto dto)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);

            if (amenity == null || amenity.IsDeleted)
            {
                throw new KeyNotFoundException($"Amenity whit Id '{id}' not found.");
            }

            // 1. Mapeo Manual:
            amenity.AmenityName = dto.AmenityName;
            amenity.Description = dto.Description;
            amenity.Capacity = dto.Capacity;
            amenity.Schedules = dto.Schedules;
            amenity.Status = dto.Status;


            amenity.LastModifiedBy = "SYSTEM";

            // 2. Persistir y Guardar
            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();


            return AmenityMapper.ToAmenityResponseDto(amenity);

        }

        public async Task<bool> DeleteAmenityAsync(Guid id)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);

            if (amenity == null)
            {
                throw new KeyNotFoundException($"Amenity whit Id '{id}' not found");
            }


            if (amenity.IsDeleted)
            {
                return true;
            }

            amenity.DeletedAt = DateTime.UtcNow;

            amenity.Status = "Deleted";
            amenity.LastModifiedBy = "SYSTEM";

            amenity.LastModifiedAt = DateTime.UtcNow;

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<AmenityResponseDto> GetByIdAsync(Guid id)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);
            if (amenity == null || amenity.IsDeleted)
            {
                throw new KeyNotFoundException($"Amenity whit Id '{id}' not found");
            }
            return AmenityMapper.ToAmenityResponseDto(amenity);
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAllAsync(int page, int size)
        {
            var amenities = await _amenityRepository.GetAllAsync(page, size);
            return amenities
                .Where(a => !a.IsDeleted)
                .Select(a => AmenityMapper.ToAmenityResponseDto(a));
        }

        public async Task<AmenityResponseDto> GetAmenityByNameAsync(string amenityName)
        {
            var amenity = await _amenityRepository.GetAmenityByNameAsync(amenityName);
            if (amenity == null || amenity.IsDeleted)
            {
                throw new KeyNotFoundException($"Amenity whit name '{amenityName}' not found");
            }
            return AmenityMapper.ToAmenityResponseDto(amenity);
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAllAmenitiesWithTurnsAsync(int pageNumber, int pageSize)
        {
            var amenities = await _amenityRepository.GetAllAmenitiesWithTurnsAsync(pageNumber, pageSize);
            return amenities
                .Where(a => !a.IsDeleted)
                .Select(a => AmenityMapper.ToAmenityResponseDto(a));
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByCapacityAsync(int minimumCapacity)
        {
            
            var allAmenities = await _amenityRepository.GetAllAsync(1, 10000);
            return allAmenities
                .Where(a => !a.IsDeleted && a.Capacity >= minimumCapacity)
                .Select(a => AmenityMapper.ToAmenityResponseDto(a));
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByStatusAsync(string status)
        {
            var amenities = await _amenityRepository.GetAmenitiesByStatusAsync(status);
            return amenities
                .Where(a => !a.IsDeleted)
                .Select(a => AmenityMapper.ToAmenityResponseDto(a));
        }
    }
}