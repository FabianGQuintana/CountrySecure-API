using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Mappers;

namespace CountrySecure.Application.Services
{
    public class AmenityService : IAmenityService
    {
        // Dependencias
        private readonly IAmenityRepository _amenityRepository;
        private readonly ITurnRepository _turnRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IAmenityRepository amenityRepository, ITurnRepository turnRepository, IUnitOfWork unitOfWork)
        {
            _amenityRepository = amenityRepository;
            _turnRepository = turnRepository;
            _unitOfWork = unitOfWork;
        }

        // --- MÉTODOS DE ESCRITURA ---

        public async Task<AmenityResponseDto> AmenityCreateAsync(AmenityCreateDto dto, Guid createdById)
        {
            var existingAmenity = await _amenityRepository.GetAmenityByNameAsync(dto.AmenityName);
            if (existingAmenity != null && !existingAmenity.IsDeleted)
            {
                throw new InvalidOperationException($"Amenity with name '{dto.AmenityName}' already exists.");
            }

            // Mapeo DTO -> Entidad (usa el método de extensión ToEntity)
            var amenity = dto.ToEntity();

            // Asignación de auditoría/estado
            amenity.CreatedBy = createdById.ToString();
            amenity.Status = "Active";

            await _amenityRepository.AddAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            // Usa el método de extensión ToResponseDto
            return amenity.ToResponseDto();
        }

        
        public async Task<AmenityResponseDto?> AmenityUpdateAsync(Guid id, AmenityUpdateDto dto, Guid currentUserId)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);

            if (amenity == null)
            {
                return null; // Devuelve null si no se encuentra (para el 404 del Controller)
            }

            // 1. Mapeo: Aplicar solo los cambios del DTO
            dto.MapToEntity(amenity);

            // 2. Actualizar Auditoría
            amenity.LastModifiedBy = currentUserId.ToString();
            amenity.LastModifiedAt = DateTime.UtcNow;

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }

        // COINCIDENCIA DE FIRMA CON LA INTERFAZ: Se corrige para que reciba currentUserId
        // AmenityService.ToggleActiveAsync (ejemplo)
        public async Task<AmenityResponseDto?> ToggleActiveAsync(Guid id, Guid currentUserId)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);
            if (amenity == null) return null;

            if (amenity.DeletedAt == null)
            {
                // desactivar
                amenity.DeletedAt = DateTime.UtcNow;
                amenity.Status = "Inactive";
            }
            else
            {
               
                // activar
                amenity.DeletedAt = null;
                amenity.Status = "Active";
            }

            amenity.LastModifiedBy = currentUserId.ToString();
            amenity.LastModifiedAt = DateTime.UtcNow;

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }



        // --- MÉTODOS DE LECTURA ---

        public async Task<AmenityResponseDto?> GetByIdAsync(Guid id)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);
            if (amenity == null || amenity.IsDeleted)
            {
                throw new KeyNotFoundException($"Amenity with Id '{id}' not found");
            }
            return amenity.ToResponseDto();
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAllAsync(int page, int size)
        {
            var amenities = await _amenityRepository.GetAllAsync(page, size);
            return amenities
                .Where(a => !a.IsDeleted)
                .ToResponseDto();
        }

        public async Task<AmenityResponseDto?> GetAmenityByNameAsync(string amenityName)
        {
            var amenity = await _amenityRepository.GetAmenityByNameAsync(amenityName);
            if (amenity == null || amenity.IsDeleted)
            {
                throw new KeyNotFoundException($"Amenity with name '{amenityName}' not found");
            }
            return amenity.ToResponseDto();
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAllAmenitiesWithTurnsAsync(int pageNumber, int pageSize)
        {
            var amenities = await _amenityRepository.GetAllAmenitiesWithTurnsAsync(pageNumber, pageSize);
            return amenities
                .Where(a => !a.IsDeleted)
                .ToResponseDto();
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByCapacityAsync(int minimumCapacity)
        {
            var allAmenities = await _amenityRepository.GetAllAsync(1, 10000);
            return allAmenities
                .Where(a => !a.IsDeleted && a.Capacity >= minimumCapacity)
                .ToResponseDto();
        }

        public async Task<IEnumerable<AmenityResponseDto>> GetAmenitiesByStatusAsync(string status)
        {
            var amenities = await _amenityRepository.GetAmenitiesByStatusAsync(status);
            return amenities
                .Where(a => !a.IsDeleted)
                .ToResponseDto();
        }
    }
}