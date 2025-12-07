using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public required string Description { get; set; }
        public required string SupplierName { get; set; }
        public required OrderStatus OrderType { get; set; }

        // Relación opcional
        public List<Guid>? RequestIds { get; set; }
        public List<Guid>? EntryPermissionIds { get; set; }
    }
}
