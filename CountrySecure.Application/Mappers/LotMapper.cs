using CountrySecure.Application.DTOs.Lots;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System; 
using System.Collections.Generic;
using System.Linq;

namespace CountrySecure.Application.Mappers
{
    public static class LotMapper
    {
        // 1. Entidad -> DTO de Respuesta (Lectura/Consulta)
        public static LotResponseDto ToResponseDto(this Lot lot)
        {
            return new LotResponseDto
            {
                LotId = lot.Id, 
                LotName = lot.LotName,
                BlockName = lot.BlockName,
                Status = Enum.TryParse<LotStatus>(lot.Status, true, out var statusResult) ? statusResult : LotStatus.Available,
                CreatedAt = lot.CreatedAt, // Desde BaseEntity
            };
        }

        // 2. Colección de Entidades -> Colección de DTOs (Consulta de lista)
        public static IEnumerable<LotResponseDto> ToResponseDto(this IEnumerable<Lot> lots)
        {
            return lots.Select(l => l.ToResponseDto());
        }

        // 3. DTO de Creación -> Entidad (Escritura POST)
        public static Lot ToEntity(this CreateLotDto dto)
        {
            // Nota: Los campos required (Status, CreatedBy) deben ser inicializados
            return new Lot
            {
                LotName = dto.LotName,
                BlockName = dto.BlockName
                // Status y CreatedBy se manejarán en la capa de Servicio o con un Hook de EF Core
            };
        }

        // 4. Mapeo de Actualización (Actualización Parcial PUT/PATCH)
        public static void MapToEntity(this UpdateLotDto dto, Lot existingEntity)
        {
            // Solo sobrescribe si el valor del DTO NO es nulo
            existingEntity.LotName = dto.LotName ?? existingEntity.LotName;
            existingEntity.BlockName = dto.BlockName ?? existingEntity.BlockName;

            // Los campos de enum deben manejarse con cuidado (asumiendo que LotStatus es nullable en el DTO)
            if (dto.Status.HasValue)
            {
                existingEntity.Status = dto.Status.ToString()!; // Convertir el Enum a string para la DB
            }
        }
    }
}