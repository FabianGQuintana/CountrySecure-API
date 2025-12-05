using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CountrySecure.Application.Mappers
{
    // Clase estática para métodos de extensión de mapeo
    public static class EntryPermissionMapper
    {
        // -------------------------------------------------------------------
        // Mapeo de SALIDA (Lectura: Entidad -> Response DTO)
        // -------------------------------------------------------------------

        public static EntryPermissionResponseDto ToResponseDto(this EntryPermission permission)
        {
            // NOTA: Si la Entidad no está cargada con Include(), esto fallará.

            // 1. Validar que las entidades requeridas estén cargadas (Eager Loading)
            if (permission.User == null || permission.Visit == null)
            {
                throw new InvalidOperationException("Cannot map EntryPermission to Response DTO. Required navigation properties (User or Visit) were not loaded from the database.");
            }

            return new EntryPermissionResponseDto
            {
                // Propiedades principales
                Id = permission.Id,
                QrCodeValue = permission.QrCodeValue,
                Type = permission.PermissionType,

                // Propiedades de Auditoría (Mapeadas de la Entidad Base)
                CreatedAt = permission.CreatedAt,
                CreatedBy = permission.CreatedBy,

                // 2. Mapeo de DTOs Anidados (Llama a los mappers de User/Visit/Service)
                // Usamos el operador ternario para el Servicio, que es opcional (nulleable)
                Resident = permission.User.ToEntryPermissionUserDto(), // Llama a UserMapper.ToEntryPermissionUserDto

                Visitor = permission.Visit.ToEntryPermissionVisitDto(), // Llama a VisitMapper.ToEntryPermissionVisitDto

                // Service = permission.Service != null
                //             ? permission.Service.ToEntryPermissionServiceDto()
                //             : null // Retorna null si Service no fue cargado o ServiceId es null
            };
        }

        public static IEnumerable<EntryPermissionResponseDto> ToResponseDto(this IEnumerable<EntryPermission> permissions)
        {
            return permissions.Select(p => p.ToResponseDto());
        }

        // -------------------------------------------------------------------
        // Mapeo de ENTRADA (Escritura: Create DTO -> Entidad)
        // -------------------------------------------------------------------

        public static EntryPermission ToEntity(this CreateEntryPermissionDto dto)
        {
            return new EntryPermission
            {
                // Mapeo de propiedades simples
                QrCodeValue = dto.QrCodeValue,
                PermissionType = dto.PermissionType,
                Description = dto.Description,

                // Asignación de Claves Foráneas (FKs)
                // Se asume que el DTO solo proporciona los IDs
                UserId = dto.UserId,
                VisitId = dto.VisitId,
                ServiceId = dto.ServiceId, // Puede ser null
            };
        }

        // -------------------------------------------------------------------
        // Mapeo de ACTUALIZACIÓN (Actualización: Update DTO -> Entidad Existente)
        // -------------------------------------------------------------------

        public static void MapToEntity(this UpdateEntryPermissionDto dto, EntryPermission existingEntity)
        {
            // Solo actualiza si el valor del DTO no es null (Para el patrón PATCH/PUT)
            existingEntity.QrCodeValue = dto.QrCodeValue ?? existingEntity.QrCodeValue;
            existingEntity.Description = dto.Description ?? existingEntity.Description;

            // Actualización del Enum (usamos .HasValue si el enum es nulleable en el DTO)
            if (dto.PermissionType.HasValue)
            {
                existingEntity.PermissionType = dto.PermissionType.Value;
            }

            // Actualización de FKs si son nulleables en el DTO (Guid?)
            if (dto.VisitId.HasValue)
            {
                existingEntity.VisitId = dto.VisitId.Value;
            }
            if (dto.UserId.HasValue)
            {
                existingEntity.UserId = dto.UserId.Value;
            }
            if (dto.ServiceId.HasValue)
            {
                existingEntity.ServiceId = dto.ServiceId.Value;
            }
        }
    }
}