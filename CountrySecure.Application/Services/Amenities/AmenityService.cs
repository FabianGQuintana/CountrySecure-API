using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Exceptions;

namespace CountrySecure.Application.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IAmenityRepository amenityRepository, IUnitOfWork unitOfWork)
        {
            _amenityRepository = amenityRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AmenityResponseDto> CreateAsync(AmenityCreateDto dto, Guid currentUserId)
        {
            if (await _amenityRepository.GetByNameAsync(dto.AmenityName) != null)
                throw new ConflictException($"Ya existe un amenity llamado '{dto.AmenityName}'.");

            var amenity = dto.ToEntity();
            amenity.CreatedBy = currentUserId.ToString();

            await _amenityRepository.AddAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }

        public async Task<AmenityResponseDto> UpdateAsync(Guid id, AmenityUpdateDto dto, Guid currentUserId)
        {
            var amenity = await GetOrThrowAsync(id);

            if (dto.AmenityName != null && dto.AmenityName != amenity.AmenityName)
            {
                var sameName = await _amenityRepository.GetByNameAsync(dto.AmenityName);
                if (sameName != null && sameName.Id != id)
                    throw new ConflictException($"Ya existe un amenity llamado '{dto.AmenityName}'.");
            }

            dto.MapToEntity(amenity);
            amenity.Touch(currentUserId.ToString());

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }

        public async Task<AmenityResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var amenity = await GetOrThrowAsync(id);
            amenity.SoftDelete(currentUserId.ToString());

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }

        public async Task<AmenityResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var amenity = await _amenityRepository.GetByIdIncludingDeletedAsync(id)
                          ?? throw new KeyNotFoundException($"No se encontró el amenity {id}.");

            amenity.Restore(currentUserId.ToString());

            await _amenityRepository.UpdateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();

            return amenity.ToResponseDto();
        }

        public async Task<AmenityResponseDto> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            // El Admin lista también los amenities dados de baja y puede abrir su detalle para reactivarlos
            var amenity = includeInactive
                ? await _amenityRepository.GetByIdIncludingDeletedAsync(id)
                : await _amenityRepository.GetByIdAsync(id);

            return (amenity ?? throw new KeyNotFoundException($"No se encontró el amenity {id}.")).ToResponseDto();
        }

        public async Task<PagedResult<AmenityResponseDto>> GetPagedAsync(PageQuery paging, string? search, int? minCapacity, bool includeInactive)
        {
            var page = await _amenityRepository.GetPagedAsync(paging, search, minCapacity, includeInactive);
            return page.Map(a => a.ToResponseDto());
        }

        private async Task<Domain.Entities.Amenity> GetOrThrowAsync(Guid id)
            => await _amenityRepository.GetByIdAsync(id)
               ?? throw new KeyNotFoundException($"No se encontró el amenity {id}.");
    }
}
