using System;
using System.Collections.Generic;
using System.Linq;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Visits;

namespace CountrySecure.Application.Mappers
{
    public static class VisitMapper
    {
        // ============================================================
        // 1. ENTIDAD -> DTO (Lectura)
        // ============================================================
        public static VisitResponseDto ToResponseDto(this Visit visit)
        {
            return new VisitResponseDto
            {
                VisitId = visit.Id,
                NameVisit = visit.NameVisit,
                LastNameVisit = visit.LastNameVisit,
                DniVisit = visit.DniVisit,
            };
        }

        // Colección -> DTO Colección
        public static IEnumerable<VisitResponseDto> ToResponseDto(this IEnumerable<Visit> visits)
        {
            return visits.Select(v => v.ToResponseDto());
        }

        // ============================================================
        // 2. DTO CREACIÓN -> Entidad
        // ============================================================
        public static Visit ToEntity(this CreateVisitDto dto)
        {
            return new Visit
            {
                NameVisit = dto.NameVisit,
                LastNameVisit = dto.LastNameVisit,
                DniVisit = dto.DniVisit,
            };
        }

        // ============================================================
        // 3. DTO ACTUALIZACIÓN -> SOBRE ENTIDAD EXISTENTE
        // ============================================================
        public static void MapToEntity(this UpdateVisitDto dto, Visit existingEntity)
        {
            existingEntity.NameVisit = dto.NameVisit ?? existingEntity.NameVisit;
            existingEntity.LastNameVisit = dto.LastNameVisit ?? existingEntity.LastNameVisit;

            if (dto.DniVisit.HasValue)
                existingEntity.DniVisit = dto.DniVisit.Value;

           
        }
    }
}
