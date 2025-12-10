using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Properties;
using System.Collections.Generic;
using System.Linq;
using System;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Mappers
{
    public static class PropertyMapper
    {
        // 1. Entidad -> DTO de Respuesta (Lectura/Consulta)
        public static PropertyResponseDto ToResponseDto(this Property property)
        {

            return new PropertyResponseDto
            {
                PropertyId = property.Id,
                Street = property.Street,
                PropertyNumber = property.PropertyNumber,
                // **Mapeo del Estado Funcional (El Enum)**
                PropertyStatus = property.PropertyStatus,

                // **Mapeo del Estado de Auditoría (El String de BaseEntity)**
                StatusAuditoria = property.Status,
                CreatedAt = property.CreatedAt,

                LotName = property.Lot?.LotName,
                OwnerName = property.User != null ? $"{property.User.Name} {property.User.Lastname}": null
            };
        }

        // 2. Colección de Entidades -> Colección de DTOs
        public static IEnumerable<PropertyResponseDto> ToResponseDto(this IEnumerable<Property> properties)
        {
            return properties.Select(p => p.ToResponseDto());
        }

        // 3. DTO de Creación -> Entidad (Escritura POST)
        public static Property ToEntity(this CreatePropertyDto dto)
        {
            return new Property
            {
                // Mapeo de propiedades simples
                Street = dto.Street,
                PropertyNumber = dto.PropertyNumber,
                PropertyStatus = PropertyStatus.NewBrand, // Estado inicial predeterminado
                // Asignación de Claves Foráneas (FKs)
                // El UserId se omite aquí y será NULL en la entidad
                LotId = dto.LotId,

                // **CRÍTICO: Las propiedades de navegación deben ser NULL**
                User = null,
                Lot = null
            };
        }

        // 4. Mapeo de Actualización (Actualización Parcial PUT/PATCH)
        public static void MapToEntity(this UpdatePropertyDto dto, Property existingEntity)
        {
            // Solo sobrescribe si el valor del DTO NO es nulo
            existingEntity.Street = dto.Street ?? existingEntity.Street;


            if (dto.PropertyStatus.HasValue)
            {
                existingEntity.PropertyStatus = dto.PropertyStatus.Value; 
            }

            // Tipo de valor (int?) requiere una comprobación HasValue
            if (dto.PropertyNumber.HasValue)
            {
                existingEntity.PropertyNumber = dto.PropertyNumber.Value;
            }

            // Si quieres permitir cambiar el usuario/lote:
            if (dto.UserId.HasValue)
            {
                existingEntity.UserId = dto.UserId.Value;
            }
            if (dto.LotId.HasValue)
            {
                existingEntity.LotId = dto.LotId.Value;
            }

            if (!string.IsNullOrEmpty(dto.StatusAuditoria))
            {
                existingEntity.Status = dto.StatusAuditoria;
            }

            // Nota: La actualización del Status se manejará con métodos específicos (ej. SoftDelete)
        }
    }
}