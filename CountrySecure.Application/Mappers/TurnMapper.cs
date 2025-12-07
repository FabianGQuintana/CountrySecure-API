using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System;
using CountrySecure.Application.DTOs.Amenities; // Necesitas los DTOs de referencia
using CountrySecure.Application.DTOs.Users;     // Necesitas los DTOs de referencia

namespace CountrySecure.Application.Mappers
{
    // Clase estática para métodos de extensión de mapeo de Turn
    public static class TurnMapper
    {
        // -------------------------------------------------------------------
        // Mapeo de SALIDA (Lectura: Entidad -> Response DTO)
        // -------------------------------------------------------------------

        public static TurnResponseDto ToResponseDto(this Turn turn)
        {
            // Validar que las entidades requeridas estén cargadas (Eager Loading)
            if (turn.User == null || turn.Amenity == null)
            {
                throw new InvalidOperationException("Cannot map Turn to Response DTO. Required navigation properties (User or Amenity) were not loaded from the database.");
            }

            return new TurnResponseDto
            {
                // Propiedades principales
                Id = turn.Id,
                StartTime = turn.StartTime,
                EndTime = turn.EndTime,

                // Mapeo del Enum a String (Recomendado para APIs)
                Status = turn.Status.ToString(),

                // Claves Foráneas
                UserId = turn.UserId,
                AmenityId = turn.AmenityId,

                // Propiedades de Auditoría
                CreatedAt = turn.CreatedAt,
                CreatedBy = turn.CreatedBy,

                // Mapeo de DTOs Anidados (Asumo que existen mappers para estas referencias)
                User = new UserReferenceDto // Usamos la estructura definida previamente
                {
                    Id = turn.User.Id,
                    FullName = $"{turn.User.Name} {turn.User.Lastname}" // Asume que la entidad User tiene 'nombre' y 'apellido'
                },
                Amenity = new AmenityReferenceDto // Usamos la estructura definida previamente
                {
                    Id = turn.Amenity.Id,
                    Name = turn.Amenity.AmenityName // Asume que la entidad Amenity tiene 'nombre'
                }
            };
        }

        public static IEnumerable<TurnResponseDto> ToResponseDto(this IEnumerable<Turn> turns)
        {
            return turns.Select(t => t.ToResponseDto());
        }

        // -------------------------------------------------------------------
        // Mapeo de ENTRADA (Escritura: Create DTO -> Entidad)
        // -------------------------------------------------------------------

        public static Turn ToEntity(this CreateTurnDto dto)
        {
            return new Turn
            {
                // Mapeo de propiedades simples
                AmenityId = dto.AmenityId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,

                // Nota: UserId, CreatedBy, CreatedAt y Status serán asignados en el servicio
                // (UserId viene del token, Status se asigna a TurnStatus.Pending)
            };
        }

        // -------------------------------------------------------------------
        // Mapeo de ACTUALIZACIÓN (Actualización: Update DTO -> Entidad Existente)
        // -------------------------------------------------------------------

        // public static void MapToEntity(this UpdateTurnDto dto, Turn existingEntity)
        // {
        //     // Solo actualiza si el valor del DTO no es null

        //     // Actualización de DateTime (usamos .HasValue si fueran nulleables)
        //     if (dto.StartTime.HasValue)
        //     {
        //         existingEntity.StartTime = dto.StartTime.Value;
        //     }
        //     if (dto.EndTime.HasValue)
        //     {
        //         existingEntity.EndTime = dto.EndTime.Value;
        //     }

        //     // Actualización del Status (Mapeo de String a Enum)
        //     if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<TurnStatus>(dto.Status, true, out var newStatus))
        //     {
        //         existingEntity.Status = newStatus;
        //     }
        // }
    }
}