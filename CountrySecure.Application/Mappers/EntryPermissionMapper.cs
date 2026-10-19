using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Mappers
{
    public static class EntryPermissionMapper
    {
        // Entidad -> DTO de respuesta
        public static EntryPermissionResponseDto ToResponseDto(this EntryPermission permission)
        {
            return new EntryPermissionResponseDto
            {
                Id = permission.Id,
                QrCodeValue = permission.QrCodeValue,
                Type = permission.PermissionType,
                Status = permission.EntryPermissionState,
                PassType = permission.PassType,
                CompanionsCount = permission.CompanionsCount,
                VehicleType = permission.VehicleType,
                VehiclePlate = permission.VehiclePlate,
                VehicleModel = permission.VehicleModel,
                ValidFrom = permission.ValidFrom,
                EntryTime = permission.EntryTime,
                DepartureTime = permission.DepartureTime,
                ValidTo = permission.ValidTo,
                CreatedAt = permission.CreatedAt,
                CreatedBy = permission.CreatedBy,
                BaseEntityStatus = permission.ToAuditStatus(),

                // User/Visit pueden ser null si fueron dados de baja (el filtro global los oculta del Include)
                Resident = permission.User?.ToEntryPermissionUserDto() ?? DeletedResident,
                Visitor = permission.Visit?.ToEntryPermissionVisitDto() ?? DeletedVisitor,
                Order = permission.Order?.ToEntryPermissionOrderDto(),

                CheckInGuard = permission.CheckInGuard?.ToGuardDto(),
                CheckOutGuard = permission.CheckOutGuard?.ToGuardDto()
            };
        }

        public static IEnumerable<EntryPermissionResponseDto> ToResponseDto(this IEnumerable<EntryPermission> permissions)
            => permissions.Select(p => p.ToResponseDto());

        // DTO de creación -> entidad
        public static EntryPermission ToEntity(this CreateEntryPermissionDto dto)
        {
            return new EntryPermission
            {
                QrCodeValue = string.Empty, // lo asigna el servicio
                PermissionType = dto.PermissionType,
                Description = dto.Description,
                CompanionsCount = dto.CompanionsCount,
                VehicleType = dto.VehicleType,
                VehiclePlate = NormalizePlate(dto.VehicleType, dto.VehiclePlate),
                VehicleModel = string.IsNullOrWhiteSpace(dto.VehicleModel) ? null : dto.VehicleModel.Trim(),
                PassType = dto.PassType,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                EntryPermissionState = PermissionStatus.Pending,
                UserId = dto.UserId,
                VisitId = dto.VisitId,
                OrderId = dto.OrderId
            };
        }

        // DTO de edición -> entidad existente (solo los campos enviados)
        public static void MapToEntity(this UpdateEntryPermissionDto dto, EntryPermission existing)
        {
            if (dto.PermissionType.HasValue) existing.PermissionType = dto.PermissionType.Value;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.CompanionsCount.HasValue) existing.CompanionsCount = dto.CompanionsCount.Value;
            if (dto.PassType.HasValue) existing.PassType = dto.PassType.Value;
            if (dto.VehicleType.HasValue) existing.VehicleType = dto.VehicleType.Value;
            if (dto.VehiclePlate != null || dto.VehicleType.HasValue)
                existing.VehiclePlate = NormalizePlate(existing.VehicleType, dto.VehiclePlate ?? existing.VehiclePlate);
            if (dto.VehicleModel != null)
                existing.VehicleModel = string.IsNullOrWhiteSpace(dto.VehicleModel) ? null : dto.VehicleModel.Trim();
            if (dto.ValidFrom.HasValue) existing.ValidFrom = dto.ValidFrom.Value;
            if (dto.ValidTo.HasValue) existing.ValidTo = dto.ValidTo.Value;
            if (dto.OrderId.HasValue) existing.OrderId = dto.OrderId.Value;
        }

        /// <summary>La patente se guarda en mayúsculas y sin espacios; a pie no hay patente.</summary>
        private static string? NormalizePlate(VehicleType type, string? plate)
            => type == VehicleType.OnFoot || string.IsNullOrWhiteSpace(plate)
                ? null
                : plate.Replace(" ", "").Replace("-", "").ToUpperInvariant();

        public static EntryPermissionOrderDto ToEntryPermissionOrderDto(this Order order)
        {
            return new EntryPermissionOrderDto
            {
                Id = order.Id,
                SupplierName = order.SupplierName,
                Description = order.Description,
                OrderType = order.Category,
                Status = order.ToAuditStatus()
            };
        }

        private static EntryPermissionResponseDto.EntryPermissionGuardDto ToGuardDto(this User guard)
            => new() { Id = guard.Id, Name = guard.Name, Lastname = guard.Lastname, dni = guard.Dni };

        private static readonly EntryPermissionUserDto DeletedResident = new()
        {
            Id = Guid.Empty, Name = "(residente", Lastname = "eliminado)", Dni = 0, Phone = "", Email = ""
        };

        private static readonly EntryPermissionVisitDto DeletedVisitor = new()
        {
            Id = Guid.Empty, NameVisit = "(visitante", LastNameVisit = "eliminado)", DniVisit = 0
        };
    }
}
