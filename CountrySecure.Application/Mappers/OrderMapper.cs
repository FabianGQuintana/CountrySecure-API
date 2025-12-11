using System;
using System.Collections.Generic;
using System.Linq;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Mappers
{
    public static class OrderMapper
    {
        // ============================================================
        // 1. Entidad -> DTO de Respuesta (Lectura)
        // ============================================================
        public static OrderResponseDto ToResponseDto(this Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                Description = order.Description,
                SupplierName = order.SupplierName,
                OrderType = order.OrderType,
                Status = order.Status,
                

                // Relaciones → listas de GUID
                Requests = order.Requests?.Select(r => r.Id).ToList() ?? new List<Guid>(),
                EntryPermissions = order.EntryPermissions?.Select(ep => ep.Id).ToList() ?? new List<Guid>()
            };
        }

        // ============================================================
        // 2. Colección de Entidades -> Colección de DTOs
        // ============================================================
        public static IEnumerable<OrderResponseDto> ToResponseDto(this IEnumerable<Order> orders)
        {
            return orders.Select(o => o.ToResponseDto());
        }

        // ============================================================
        // 3. DTO de Creación -> Entidad (POST)
        // ============================================================
        public static Order ToEntity(this CreateOrderDto dto)
        {
            return new Order
            {
                Description = dto.Description,
                SupplierName = dto.SupplierName,
                OrderType = dto.OrderType,

                // Relaciones se manejarán en el servicio
                Requests = new List<Request>(),
                EntryPermissions = new List<EntryPermission>(),

            };
        }

        // ============================================================
        // 4. Actualización Parcial (PUT/PATCH)
        // ============================================================
        public static void MapToEntity(this UpdateOrderDto dto, Order existing)
        {
            // Solo sobrescribe si NO viene null
            existing.Description = dto.Description ?? existing.Description;
            existing.SupplierName = dto.SupplierName ?? existing.SupplierName;

            if (dto.OrderType.HasValue)
            {
                existing.OrderType = dto.OrderType.Value;
            }

            // Si querés permitir actualizar EntryPermissionState
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                existing.Status = dto.Status;
            }

            // Relaciones se manejan en el servicio
        }
    }
}
