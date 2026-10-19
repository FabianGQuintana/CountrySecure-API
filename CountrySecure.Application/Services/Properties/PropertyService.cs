using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;

namespace CountrySecure.Application.Services.Properties
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILotRepository _lotRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PropertyService(IPropertyRepository propertyRepository, ILotRepository lotRepository, IUnitOfWork unitOfWork)
        {
            _propertyRepository = propertyRepository;
            _lotRepository = lotRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PropertyResponseDto> CreateAsync(CreatePropertyDto dto, Guid currentUserId)
        {
            await EnsureLotExistsAsync(dto.LotId);
            await EnsureAddressIsFreeAsync(dto.Street, dto.PropertyNumber, exceptPropertyId: null);

            var property = dto.ToEntity();
            property.CreatedBy = currentUserId.ToString();
            property.PropertyStatus = PropertyStatus.NewBrand;

            await _propertyRepository.AddAsync(property);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(property.Id);
        }

        public async Task<PropertyResponseDto> UpdateAsync(Guid id, UpdatePropertyDto dto, Guid currentUserId)
        {
            var property = await _propertyRepository.GetByIdAsync(id)
                           ?? throw new KeyNotFoundException($"Property with ID {id} not found.");

            if (dto.LotId.HasValue && dto.LotId != property.LotId)
                await EnsureLotExistsAsync(dto.LotId.Value);

            var street = dto.Street ?? property.Street;
            var number = dto.PropertyNumber ?? property.PropertyNumber;
            if (street != property.Street || number != property.PropertyNumber)
                await EnsureAddressIsFreeAsync(street, number, exceptPropertyId: id);

            dto.MapToEntity(property);
            property.Touch(currentUserId.ToString());

            await _propertyRepository.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public async Task<PropertyResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var property = await _propertyRepository.GetByIdAsync(id)
                           ?? throw new KeyNotFoundException($"Property with ID {id} not found.");

            property.SoftDelete(currentUserId.ToString());
            property.PropertyStatus = PropertyStatus.Inactive;

            await _propertyRepository.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id, includeDeleted: true);
        }

        public async Task<PropertyResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var property = await _propertyRepository.GetByIdIncludingDeletedAsync(id)
                           ?? throw new KeyNotFoundException($"Property with ID {id} not found.");

            property.Restore(currentUserId.ToString());
            property.PropertyStatus = PropertyStatus.Available;

            await _propertyRepository.UpdateAsync(property);
            await _unitOfWork.SaveChangesAsync();

            return await ReloadAsync(id);
        }

        public Task<PropertyResponseDto> GetByIdAsync(Guid id) => ReloadAsync(id);

        public async Task<PagedResult<PropertyResponseDto>> GetPagedAsync(
            PageQuery paging, PropertyStatus? status, Guid? lotId, Guid? ownerId, string? search, bool includeInactive)
        {
            var page = await _propertyRepository.GetPagedAsync(paging, status, lotId, ownerId, search, includeInactive);
            return page.Map(p => p.ToResponseDto());
        }

        private async Task<PropertyResponseDto> ReloadAsync(Guid id, bool includeDeleted = false)
        {
            var property = await _propertyRepository.GetByIdWithIncludesAsync(id, includeDeleted)
                           ?? throw new KeyNotFoundException($"Property with ID {id} not found.");
            return property.ToResponseDto();
        }

        private async Task EnsureLotExistsAsync(Guid lotId)
        {
            var lot = await _lotRepository.GetByIdAsync(lotId);
            if (lot == null)
                throw new KeyNotFoundException($"The lot with ID {lotId} does not exist or is inactive.");
        }

        // La dirección es única en toda la tabla (incluidas las dadas de baja)
        private async Task EnsureAddressIsFreeAsync(string street, int number, Guid? exceptPropertyId)
        {
            var existing = await _propertyRepository.GetByAddressIncludingDeletedAsync(street, number);
            if (existing != null && existing.Id != exceptPropertyId)
            {
                throw new ConflictException(existing.IsDeleted
                    ? $"A property at {street} {number} already exists but is inactive. Restore it instead."
                    : $"A property at {street} {number} already exists.");
            }
        }
    }
}
