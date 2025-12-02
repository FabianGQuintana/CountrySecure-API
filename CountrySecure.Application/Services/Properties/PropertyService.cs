using CountrySecure.Application.Mappers;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Application.Interfaces.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CountrySecure.Application.Services.Properties
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PropertyService(IPropertyRepository propertyRepository, IUnitOfWork unitOfWork)
        {
            _propertyRepository = propertyRepository;
            _unitOfWork = unitOfWork;
        }

        // --- MÉTODOS DE ESCRITURA ---

        public async Task<PropertyResponseDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto, Guid currentUserId)
        {
            // 1. Convertir DTO a Entidad
            var newPropertyEntity = newPropertyDto.ToEntity();

            // 2. ASIGNACIÓN DE CAMPOS REQUIRED
            newPropertyEntity.CreatedBy = currentUserId.ToString();
            newPropertyEntity.Status = "Active"; // Estado inicial
            newPropertyEntity.CreatedAt = DateTime.UtcNow;

            var addedProperty = await _propertyRepository.AddAsync(newPropertyEntity);
            await _unitOfWork.SaveChangesAsync();

            // 3. Mapeo de Entidad a DTO de Respuesta
            return addedProperty.ToResponseDto();
        }

        public async Task UpdatePropertyAsync(UpdatePropertyDto updateProperty, Guid currentId)
        {
            // Asumimos que updateProperty.PropertyId es la propiedad correcta para la ID
            var existingEntity = await _propertyRepository.GetByIdAsync(updateProperty.PropertyId);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Property with ID {updateProperty.PropertyId} not found.");
            }

            // REGLA DE NEGOCIO: Solo el creador o un Admin pueden actualizar
            if (existingEntity.CreatedBy != currentId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this property.");
            }

            // Mapeo de actualización (DRY)
            updateProperty.MapToEntity(existingEntity);

            // Actualizar Auditoría
            existingEntity.LastModifiedAt = DateTime.UtcNow;
            existingEntity.LastModifiedBy = currentId.ToString();

            await _propertyRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> SoftDeletePropertyAsync(Guid propertyId, Guid currentUserId)
        {
            var existingProperty = await _propertyRepository.GetByIdAsync(propertyId);
            if (existingProperty == null)
            {
                return false;
            }

            // REGLA DE NEGOCIO: Solo el creador o un Admin pueden eliminar
            if (existingProperty.CreatedBy != currentUserId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to delete this property.");
            }

            bool marked = await _propertyRepository.DeleteAsync(propertyId);

            if (marked)
            {
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // --- MÉTODOS DE CONSULTA ---

        public async Task<PropertyResponseDto?> GetPropertyByIdAsync(Guid propertyId)
        {
            var propertyEntity = await _propertyRepository.GetByIdAsync(propertyId);
            if (propertyEntity == null) return null;

            return propertyEntity.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetAllPropertiesAsync(int pageNumber, int pageSize)
        {
            var propertyEntities = await _propertyRepository.GetAllAsync(pageNumber, pageSize);

            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize)
        {
            var propertyEntities = await _propertyRepository.GetPropertiesByStatusAsync(status, pageNumber, pageSize);

            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByOwnerId(Guid ownerId)
        {
            var propertyEntities = await _propertyRepository.GetPropertyByIdUserAsync(ownerId);
            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto>> GetPropertiesByLotIdAsync(Guid lotId)
        {
            var propertyEntities = await _propertyRepository.GetPropertiesByLotIdAsync(lotId);
            return propertyEntities.ToResponseDto();
        }
    }
}