using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Amenity;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CountrySecure.Application.Mappers
{
    // Clase estática para métodos de extensión de mapeo de la entidad Amenity
    public static class AmenityMapper
    {
        // -------------------------------------------------------------------
        // Mapeo de SALIDA (Lectura: Entidad -> Response DTO)
        // -------------------------------------------------------------------

        public static AmenityResponseDto ToAmenityResponseDto(this Amenity entity)
        {
            // Nota: Aquí se omiten validaciones complejas de navegación (como en EntryPermission)
            // ya que AmenityResponseDto no parece requerir propiedades de navegación obligatorias.

            return new AmenityResponseDto
            {
                // Propiedades principales
                Id = entity.Id,
                AmenityName = entity.AmenityName,
                Description = entity.Description,
                Schedules = entity.Schedules,
                Capacity = entity.Capacity,
                Status = entity.Status,

                // Propiedades de Auditoría (Mapeadas de la Entidad Base)
                CreatedAt = entity.CreatedAt,
                LastModifiedAt = entity.LastModifiedAt

                // Si AmenityResponseDto tuviera una lista de Turnos, se mapearía aquí.
            };
        }

        public static IEnumerable<AmenityResponseDto> ToAmenityResponseDto(this IEnumerable<Amenity> amenities)
        {
            // Mapeo de colecciones (simplificado: aplica ToAmenityResponseDto a cada elemento)
            return amenities.Select(a => a.ToAmenityResponseDto());
        }

        // -------------------------------------------------------------------
        // Mapeo de ENTRADA (Escritura: Create DTO -> Entidad)
        // -------------------------------------------------------------------

        public static Amenity ToAmenityEntity(this AmenityCreateDto dto)
        {
            return new Amenity
            {
                // Mapeo de propiedades simples
                AmenityName = dto.AmenityName,
                Description = dto.Description,
                Schedules = dto.Schedules,
                Capacity = dto.Capacity,

                // Las propiedades de auditoría (CreatedBy, CreatedAt) son manejadas por el servicio
                // o la BaseEntity y no deben mapearse desde el DTO.

                // NOTA IMPORTANTE: Los Turnos (dto.Turns) DEBEN ser mapeados y agregados 
                // a la entidad Amenity en la capa de Servicio, 
                // ya que requieren lógica de negocio (validación, FKs).
            };
        }

        // -------------------------------------------------------------------
        // Mapeo de ACTUALIZACIÓN (Actualización: Update DTO -> Entidad Existente)
        // -------------------------------------------------------------------

        public static void MapToAmenityEntity(this AmenityUpdateDto dto, Amenity existingEntity)
        {
            // Aplicamos todos los campos requeridos en el DTO de actualización:

            existingEntity.AmenityName = dto.AmenityName;
            existingEntity.Description = dto.Description;
            existingEntity.Schedules = dto.Schedules;
            existingEntity.Capacity = dto.Capacity;
            existingEntity.Status = dto.Status;

            // La propiedad Id no se actualiza (es la clave).
            // La auditoría (LastModifiedAt, LastModifiedBy) se gestiona en la capa de Servicio.

            // NOTA: Si el DTO de actualización fuera para un PATCH (con campos nulleables), 
            // usaríamos la lógica del EntryPermissionMapper (existingEntity.Prop = dto.Prop ?? existingEntity.Prop).
            // Dado que todos los campos del AmenityUpdateDto son [Required], hacemos una asignación directa.
        }
    }
}