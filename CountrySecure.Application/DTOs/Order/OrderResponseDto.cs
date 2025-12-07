using System;
using System.Collections.Generic;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Order
{
    public class OrderResponseDto
    {
        // Atributes
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public required string SupplierName { get; set; }
        public OrderStatus OrderType { get; set; }
        public required string Status { get; set; }

        // Relationships (solo IDs )
        public List<Guid>? Requests { get; set; }
        public List<Guid>? EntryPermissions { get; set; }
    }
}
