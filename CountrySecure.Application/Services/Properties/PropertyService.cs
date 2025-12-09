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

            // Forzamos la asignación del LotId para asegurar que la FK se envíe correctamente.
            newPropertyEntity.LotId = newPropertyDto.LotId;

            var lotExists = await _lotRepository.GetByIdAsync(newPropertyDto.LotId);

            if (lotExists == null || lotExists.IsDeleted) // Verifica que exista Y no esté eliminado
            {
                // Esto lanzará la excepción que tu controlador debe atrapar para devolver 404/400
                throw new KeyNotFoundException($"El Lote con ID {newPropertyDto.LotId} no existe o está inactivo/eliminado.");
            }

            // 2. CONFIGURACIÓN DE ESTADO INICIAL Y AUDITORÍA
            newPropertyEntity.CreatedBy = currentUserId.ToString();
            newPropertyEntity.PropertyType = PropertyStatus.NewBrand;
            newPropertyEntity.Status = "Active";
            newPropertyEntity.CreatedAt = DateTime.UtcNow;
            newPropertyEntity.LastModifiedAt = DateTime.UtcNow;

            // 3. Persistencia
            var addedProperty = await _propertyRepository.AddAsync(newPropertyEntity);
            await _unitOfWork.SaveChangesAsync();

            return addedProperty.ToResponseDto();
        }

        public async Task<PropertyResponseDto?> UpdatePropertyAsync(Guid propertyId, UpdatePropertyDto updateProperty, Guid currentId)
        {
            // 1. Validar la existencia: Usamos el ID de la propiedad que viene en el parámetro (de la URL)
            var existingEntity = await _propertyRepository.GetByIdAsync(propertyId);

            // Manejo de 404 Not Found (Devolver null al controlador)
            if (existingEntity == null)
            {
                return null;
            }

            // 2. REGLA DE NEGOCIO: Validar Autorización
            // Solo el creador (o un rol superior, si lo implementas) puede modificar.
            if (existingEntity.CreatedBy != currentId.ToString())
            {
                // Lanza una excepción que será capturada por el controlador para devolver 403 Forbidden.
                throw new UnauthorizedAccessException("User is not authorized to update this property. Only the creator may modify it.");
            }

            // 3. Mapeo y Aplicación de Cambios
            // Aplica solo los campos no nulos del DTO a la entidad existente.
            updateProperty.MapToEntity(existingEntity);

            // 4. Actualizar Auditoría
            existingEntity.LastModifiedAt = DateTime.UtcNow;
            existingEntity.LastModifiedBy = currentId.ToString(); // Registra quién realizó la modificación

            // 5. Guardar Cambios
            var updatedEntity = await _propertyRepository.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync(); // Persiste la transacción

            // 6. Mapeo de Salida y Retorno
            // Devuelve el objeto actualizado al controlador.
            return updatedEntity.ToResponseDto();
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

        public async Task<IEnumerable<PropertyResponseDto?>> GetAllPropertiesAsync(int pageNumber, int pageSize)
        {
            var propertyEntities = await _propertyRepository.GetAllAsync(pageNumber, pageSize);

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