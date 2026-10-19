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
            // User/Amenity pueden ser null si fueron dados de baja (el filtro global los oculta del Include)
            return new TurnResponseDto
            {
                Id = turn.Id,
                StartTime = turn.StartTime,
                EndTime = turn.EndTime,

                // Estado de auditoría derivado de DeletedAt y estado funcional de la reserva
                Status = turn.ToAuditStatus(),
                TurnStatus = turn.TurnStatus.ToString(),

                UserId = turn.UserId,
                AmenityId = turn.AmenityId,

                CreatedAt = turn.CreatedAt,
                CreatedBy = turn.CreatedBy,

                User = turn.User == null ? null : new UserReferenceDto
                {
                    Id = turn.User.Id,
                    FullName = $"{turn.User.Name} {turn.User.Lastname}"
                },
                Amenity = turn.Amenity == null ? null : new AmenityReferenceDto
                {
                    Id = turn.Amenity.Id,
                    Name = turn.Amenity.AmenityName
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

                // Nota: UserId, CreatedBy, CreatedAt y EntryPermissionState serán asignados en el servicio
                // (UserId viene del token, EntryPermissionState se asigna a TurnStatus.Pending)
            };
        }

        //-------------------------------------------------------------------
        //Mapeo de ACTUALIZACIÓN(Actualización: Update DTO -> Entidad Existente)
        //-------------------------------------------------------------------

        public static void MapToEntity(this UpdateTurnDto dto, Turn existingEntity)
        {
            // Actualización de Tiempos
            if (dto.StartTime.HasValue)
            {
                existingEntity.StartTime = dto.StartTime.Value;
            }

            if (dto.EndTime.HasValue)
            {
                existingEntity.EndTime = dto.EndTime.Value;
            }

            // Actualización del Estado Funcional
            if (dto.TurnStatus.HasValue)
            {
                existingEntity.TurnStatus = dto.TurnStatus.Value;
            }

            // Actualización de FKs (Ahora que existen en el DTO)
            if (dto.AmenityId.HasValue)
            {
                existingEntity.AmenityId = dto.AmenityId.Value;
            }
            if (dto.UserId.HasValue)
            {
                existingEntity.UserId = dto.UserId.Value;
            }


        }
    }
}