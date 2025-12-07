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
            // Convertir el Status string (de BaseEntity) al enum PropertyStatus
            var statusEnum = PropertyStatus.Inactive;
            if (!string.IsNullOrWhiteSpace(property.Status) && Enum.TryParse<PropertyStatus>(property.Status, out var parsed))
            {
                statusEnum = parsed;
            }

            return new PropertyResponseDto
            {
                PropertyId = property.Id,
                Street = property.Street,
                HouseNumber = property.PropertyNumber,
                Status = statusEnum
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

                // Asignación de Claves Foráneas (FKs)
                UserId = dto.UserId,
                LotId = dto.LotId,

                // **CRÍTICO: Las propiedades de navegación deben ser NULL para evitar el error 23505 (PK Violation)**
                User = null,
                Lot = null
            };
        }

        // 4. Mapeo de Actualización (Actualización Parcial PUT/PATCH)
        public static void MapToEntity(this UpdatePropertyDto dto, Property existingEntity)
        {
            // Solo sobrescribe si el valor del DTO NO es nulo
            existingEntity.Street = dto.Street ?? existingEntity.Street;

            // Tipo de valor (int?) requiere una comprobación HasValue
            if (dto.NumberProperty.HasValue)
            {
                existingEntity.PropertyNumber = dto.NumberProperty.Value;
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

            // Nota: La actualización del Status se manejará con métodos específicos (ej. SoftDelete)
        }
    }
}