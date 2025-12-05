using System;
using System.Collections.Generic;
using System.Linq;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.DTOs.EntryPermission; // <-- Necesario para el DTO anidado

namespace CountrySecure.Application.Mappers
{
    public static class VisitMapper
    {
        // ============================================================
        // 1. ENTIDAD -> DTO (Lectura Principal)
        // ============================================================
        public static VisitResponseDto ToResponseDto(this Visit visit)
        {
            // Nota: Aquí estamos asumiendo que el Status es un string en la Entidad Base,
            // pero el DTO de respuesta de la visita puede requerir un tipo diferente
            return new VisitResponseDto
            {
                VisitId = visit.Id,
                NameVisit = visit.NameVisit,
                LastNameVisit = visit.LastNameVisit,
                DniVisit = visit.DniVisit,
                VisitStatus = visit.Status // Asumiendo que VisitResponseDto.VisitStatus es string
            };
        }

        // Colección -> DTO Colección
        public static IEnumerable<VisitResponseDto> ToResponseDto(this IEnumerable<Visit> visits)
        {
            return visits.Select(v => v.ToResponseDto());
        }

        // ============================================================
        // 1.5. ENTIDAD -> DTO ANIDADO (Para EntryPermission) <-- NUEVO
        // ============================================================

        /// <summary>
        /// Mapea la Entidad Visit a su versión reducida para ser anidada dentro de EntryPermissionResponseDto.
        /// </summary>
        public static EntryPermissionVisitDto ToEntryPermissionVisitDto(this Visit visit)
        {
            return new EntryPermissionVisitDto
            {
                Id = visit.Id,
                NameVisit = visit.NameVisit,
                LastNameVisit = visit.LastNameVisit,
                DniVisit = visit.DniVisit
                // Solo campos esenciales
            };
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
                // Status y CreatedBy se asignarán en el servicio o la Entidad Base
            };
        }

        // ============================================================
        // 3. DTO ACTUALIZACIÓN -> SOBRE ENTIDAD EXISTENTE
        // ============================================================
        public static void MapToEntity(this UpdateVisitDto dto, Visit existingEntity)
        {
            // Usamos ?? para strings (si dto.NameVisit es nulo, mantiene existingEntity.NameVisit)
            existingEntity.NameVisit = dto.NameVisit ?? existingEntity.NameVisit;
            existingEntity.LastNameVisit = dto.LastNameVisit ?? existingEntity.LastNameVisit;

            
            if (dto.DniVisit.HasValue)
                existingEntity.DniVisit = dto.DniVisit.Value;
        }
    }
}