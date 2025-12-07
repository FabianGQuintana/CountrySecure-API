using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Amenities;
using System.Collections.Generic;
using System.Linq;
using System;
using CountrySecure.Domain.Enums; // Asegúrate de incluir el namespace del Enum Turn

namespace CountrySecure.Application.Mappers
{
    public static class AmenityMapper
    {
        // -------------------------------------------------------------------
        // Mapeo de SALIDA (Lectura: Entidad -> Response DTO)
        // -------------------------------------------------------------------

        // Nombre del método consistente con el patrón: ToResponseDto
        public static AmenityResponseDto ToResponseDto(this Amenity entity)
        {
            return new AmenityResponseDto
            {
                Id = entity.Id,
                AmenityName = entity.AmenityName,
                Description = entity.Description,
                Schedules = entity.Schedules,
                Capacity = entity.Capacity,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                LastModifiedAt = entity.LastModifiedAt,

                // Si la colección de Turnos está cargada, la mapeamos (asumiendo que existe TurnMapper.ToResponseDto)
                // Turns = entity.Turns.Any() ? entity.Turns.ToResponseDto() : new List<TurnResponseDto>()
            };
        }

        public static IEnumerable<AmenityResponseDto> ToResponseDto(this IEnumerable<Amenity> amenities)
        {
            return amenities.Select(a => a.ToResponseDto());
        }

        // -------------------------------------------------------------------
        // Mapeo de ENTRADA (Escritura: Create DTO -> Entidad)
        // -------------------------------------------------------------------

        // Nombre del método consistente con el patrón: ToEntity
        public static Amenity ToEntity(this AmenityCreateDto dto)
        {
            return new Amenity
            {
                // Mapeo de propiedades simples
                AmenityName = dto.AmenityName,
                Description = dto.Description,
                Schedules = dto.Schedules,
                Capacity = dto.Capacity, // Directa, ya que Capacity es 'int' en el DTO

                // CRÍTICO: Inicializar colecciones de entidades para evitar errores de referencia nula.
                Turns = new List<Turn>()
            };
        }

        // -------------------------------------------------------------------
        // Mapeo de ACTUALIZACIÓN (Actualización: Update DTO -> Entidad Existente)
        // -------------------------------------------------------------------

        // Nombre del método consistente con el patrón: MapToEntity
        public static void MapToEntity(this AmenityUpdateDto dto, Amenity existingEntity)
        {
            // ELIMINACIÓN DEL ERROR DE NULIDAD (?? y HasValue): 
            // Si el DTO de actualización tiene todos los campos como [Required] (PUT):

            existingEntity.AmenityName = dto.AmenityName ?? existingEntity.AmenityName;
            existingEntity.Description = dto.Description ?? existingEntity.Description;
            existingEntity.Schedules = dto.Schedules ?? existingEntity.Schedules;
            if (dto.Capacity.HasValue)
            {
                existingEntity.Capacity = dto.Capacity.Value;
            }
            if (dto.Status != null)
            {
                existingEntity.Status = dto.Status;
            }

            // Si en el futuro cambias el DTO a nullable (string? y int?) para permitir un PATCH:
            // Tendrías que usar: existingEntity.AmenityName = dto.AmenityName ?? existingEntity.AmenityName;
            // Y: existingEntity.Capacity = dto.Capacity.HasValue ? dto.Capacity.Value : existingEntity.Capacity;
        }
    }
}