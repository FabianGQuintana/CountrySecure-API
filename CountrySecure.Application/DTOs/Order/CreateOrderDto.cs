using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public string Description { get; set; }
        public string SupplierName { get; set; }
        public OrderStatus OrderType { get; set; }

        // Relación opcional
        public List<Guid>? RequestIds { get; set; }
        public List<Guid>? EntryPermissionIds { get; set; }
    }
}
