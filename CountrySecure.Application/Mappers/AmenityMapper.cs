using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Amenity;
using System.Collections.Generic;
using System.Linq;

namespace CountrySecure.Application.Mappers
{
    // Una clase estática es ideal para funciones de utilidad como el mapeo
    public static class AmenityMapper
    {
        // Método para mapear una sola entidad a un Response DTO
        public static AmenityResponseDto ToAmenityResponseDto(Amenity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new AmenityResponseDto
            {
                // Mapeo de Propiedades
                Id = entity.Id,
                AmenityName = entity.AmenityName,
                Description = entity.Description,
                Capacity = entity.Capacity,
                Status = entity.Status,

                // Mapeo de Auditoría
                CreatedAt = entity.CreatedAt,
                LastModifiedAt = entity.LastModifiedAt
                // Aquí deberías mapear todas las propiedades que tu DTO de respuesta necesita
            };
        }

        // Opcional: Método para mapear listas de entidades a listas de Response DTOs
        public static IEnumerable<AmenityResponseDto> ToAmenityResponseDtoList(IEnumerable<Amenity> entities)
        {
            if (entities == null)
            {
                return new List<AmenityResponseDto>();
            }

            // Usamos LINQ para aplicar el mapeo a cada elemento de la lista
            return entities.Select(ToAmenityResponseDto);
        }
    }
}