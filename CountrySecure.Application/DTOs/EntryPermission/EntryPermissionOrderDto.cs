using CountrySecure.Domain.Enums;
using System;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class EntryPermissionOrderDto
    {
        public Guid Id { get; set; }

        public required string SupplierName { get; set; }

        public string? Description { get; set; }
        public OrderStatus OrderType { get; set; }

        public required string Status { get; set; }
    }
}