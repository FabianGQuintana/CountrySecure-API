using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CountrySecure.Application.DTOs.Order
{
    public class UpdateOrderDto
    {
        public string? Description { get; set; }
        public string? SupplierName { get; set; }
        public OrderStatus? OrderType { get; set; }
        public string? Status { get; set; }

        public List<Guid>? RequestIds { get; set; }
        public List<Guid>? EntryPermissionIds { get; set; }


        
    }
}
