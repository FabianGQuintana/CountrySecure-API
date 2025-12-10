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
        private readonly ILotRepository _lotRepository;
        public PropertyService(IPropertyRepository propertyRepository, IUnitOfWork unitOfWork, ILotRepository lotRepository)
        {
            _propertyRepository = propertyRepository;
            _lotRepository = lotRepository;
            _unitOfWork = unitOfWork;
        }

        // --- MÉTODOS DE ESCRITURA ---

        public async Task<PropertyResponseDto> AddNewPropertyAsync(CreatePropertyDto newPropertyDto, Guid currentUserId)
        {
            // 1. Mapeo de DTO a Entidad
            var newPropertyEntity = newPropertyDto.ToEntity();

            // 2. Validar la existencia del Lote
            // Asumo que GetByIdAsync del LotRepository obtiene la entidad si existe.
            var lotExists = await _lotRepository.GetByIdAsync(newPropertyDto.LotId);

            // CRÍTICO: Corrección de lógica para validar existencia/borrado.
            if (lotExists == null || lotExists.IsDeleted)
            {
                throw new KeyNotFoundException($"El Lote con ID {newPropertyDto.LotId} no existe o está inactivo/eliminado.");
            }

            // 3. Auditoría y Estado Inicial
            newPropertyEntity.CreatedBy = currentUserId.ToString();
            newPropertyEntity.PropertyStatus = PropertyStatus.NewBrand; // Estado funcional inicial
            newPropertyEntity.Status = "Active"; // Estado de auditoría inicial
            newPropertyEntity.CreatedAt = DateTime.UtcNow;
            newPropertyEntity.LastModifiedAt = DateTime.UtcNow;

            // 4. Persistencia (Guardar la entidad)
            var addedProperty = await _propertyRepository.AddAsync(newPropertyEntity);
            await _unitOfWork.SaveChangesAsync();


            var fullProperty = await _propertyRepository.GetByIdWithIncludesAsync(addedProperty.Id);

            if (fullProperty == null)
            {
                // En caso de fallo raro post-inserción
                throw new InvalidOperationException("La propiedad fue creada, pero no se pudo recuperar para el mapeo completo.");
            }

            // 6. Mapeo de la entidad completa (con Lot y User cargados)
            return fullProperty.ToResponseDto();
        }

        public async Task<PropertyResponseDto?> UpdatePropertyAsync(Guid propertyId, UpdatePropertyDto updateProperty, Guid currentId)
        {
            var existingEntity = await _propertyRepository.GetByIdWithIncludesAsync(propertyId);


            if (existingEntity == null)
            {
                return null; // Devuelve null para que el controlador devuelva 404 Not Found
            }

            // REGLA DE NEGOCIO: Validar Autorización
            if (existingEntity.CreatedBy != currentId.ToString())
            {
                throw new UnauthorizedAccessException("User is not authorized to update this property. Only the creator may modify it.");
            }

            // Validar FK LotId si se está actualizando (opcional, pero seguro)
            if (updateProperty.LotId.HasValue)
            {
                var lotExists = await _lotRepository.GetByIdAsync(updateProperty.LotId.Value);
                if (lotExists == null || lotExists.IsDeleted)
                {
                    throw new KeyNotFoundException($"El Lote con ID {updateProperty.LotId.Value} no existe o está inactivo/eliminado.");
                }
            }

            // Aplicar Mapeo de Cambios
            updateProperty.MapToEntity(existingEntity);

            // Actualizar Auditoría
            existingEntity.LastModifiedAt = DateTime.UtcNow;
            existingEntity.LastModifiedBy = currentId.ToString();

            // Guardar Cambios
            var updatedEntity = await _propertyRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();

            // Retorno
            return updatedEntity.ToResponseDto();
        }

        public async Task<PropertyResponseDto?> SoftDeleteAsync(Guid id)
        {
            var property = await _propertyRepository.SoftDeleteAsync(id);

            if (property == null) return null;

            await _unitOfWork.SaveChangesAsync();

            return property.ToResponseDto();
        }



        // --- MÉTODOS DE CONSULTA ---

        public async Task<PropertyResponseDto?> GetPropertyByIdAsync(Guid propertyId)
        {
            var propertyEntity = await _propertyRepository.GetByIdWithIncludesAsync(propertyId);

            if (propertyEntity == null)
                return null;

            return propertyEntity.ToResponseDto();
        }


        public async Task<IEnumerable<PropertyResponseDto?>> GetAllPropertiesAsync(int pageNumber, int pageSize)
        {
            var propertyEntities = await _propertyRepository.GetAllAsync(pageNumber, pageSize);

            // OPTIONAL: If you want navigation props → load includes one by one (costly)
            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto?>> GetPropertiesByStatusAsync(PropertyStatus status, int pageNumber, int pageSize)
        {
            var propertyEntities = await _propertyRepository.GetPropertiesByStatusAsync(status, pageNumber, pageSize);
            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto?>> GetPropertiesByOwnerId(Guid ownerId)
        {
            var propertyEntities = await _propertyRepository.GetPropertyByIdUserAsync(ownerId);
            return propertyEntities.ToResponseDto();
        }

        public async Task<IEnumerable<PropertyResponseDto?>> GetPropertiesByLotIdAsync(Guid lotId)
        {
            var propertyEntities = await _propertyRepository.GetPropertiesByLotIdAsync(lotId);
            return propertyEntities.ToResponseDto();
        }
    }
}