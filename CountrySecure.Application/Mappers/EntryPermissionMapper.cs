using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

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

                // El estado funcional de la entidad
                Status = permission.EntryPermissionState,

                ValidFrom = permission.ValidFrom,
                EntryTime = permission.EntryTime,
                DepartureTime = permission.DepartureTime,
                ValidTo = permission.ValidTo,   

                // Propiedades de Auditoría (Mapeadas de la Entidad Base)
                CreatedAt = permission.CreatedAt,
                CreatedBy = permission.CreatedBy,

              
                // Mapeamos BaseEntity.Status (el string "Active"/"Inactive") a la nueva propiedad del DTO
                BaseEntityStatus = permission.Status,
                // -------------------------

                // Mapeo de DTOs Anidados
                Resident = permission.User.ToEntryPermissionUserDto(),
                Visitor = permission.Visit.ToEntryPermissionVisitDto(),

                Order = permission.Order != null
        ? permission.Order.ToEntryPermissionOrderDto()
        : null
            };
        }

        public static IEnumerable<EntryPermissionResponseDto> ToResponseDto(this IEnumerable<EntryPermission> permissions)
        {
            return permissions.Select(p => p.ToResponseDto());
        }

        // -------------------------------------------------------------------
        // Mapeo de ENTRADA (Escritura: Create DTO -> Entidad)
        // -------------------------------------------------------------------

        // public static EntryPermission ToEntity(this CreateEntryPermissionDto dto)
        // {
        //     return new EntryPermission
        //     {
        //         QrCodeValue = string.Empty,
        //         PermissionType = dto.PermissionType,
        //         Description = dto.Description,
        //         ValidFrom = dto.ValidFrom,
        //         ValidTo = dto.ValidTo,
        //         // EntryPermissionState = dto.Status,

        //         // Asignación de Claves Foráneas (FKs)
        //         // UserId = dto.UserId,
        //         // VisitId = dto.VisitId,
        //         // OrderId = dto.OrderId,

        //         // **INICIALIZACIÓN CRÍTICA DE PROPIEDADES DE NAVEGACIÓN A NULL**
        //         Visit = null,
        //         User = null,
        //         Order = null
        //     };
        // }

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
            if (dto.OrderId.HasValue)
            {
                existingEntity.OrderId = dto.OrderId.Value;
            }

            if (dto.EntryTime.HasValue)
            {
                existingEntity.EntryTime = dto.EntryTime.Value;
            }
            if (dto.DepartureTime.HasValue)
            {
                existingEntity.DepartureTime = dto.DepartureTime.Value;
            }
            if (dto.Status.HasValue)
            {
                existingEntity.EntryPermissionState = dto.Status.Value;
            }
            // NOTA: Recuerda actualizar LastModifiedAt y LastModifiedBy en el Servicio después de llamar a MapToEntity
        }


        // -------------------------------------------------------------------
        // Mapeo de Extensión 
        // -------------------------------------------------------------------


        public static EntryPermissionOrderDto ToEntryPermissionOrderDto(this Order order)
        {
            return new EntryPermissionOrderDto
            {
                Id = order.Id,

                SupplierName = order.SupplierName,

                Description = order.Description,

                OrderType = order.OrderType,

                Status = order.Status
            };
        }
    }
}