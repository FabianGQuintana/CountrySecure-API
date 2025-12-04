using CountrySecure.Application.DTOs;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto newOrderDto, Guid currentUserId);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersWithoutFilterAsync();
        Task<IEnumerable<OrderResponseDto>> GetByStatusAsync(bool isActive);
        Task<IEnumerable<OrderResponseDto>> GetByTypeAsync(OrderStatus orderType);
        Task<IEnumerable<OrderResponseDto>> GetBySupplierAsync(string supplierName);
        Task<IEnumerable<OrderResponseDto>> GetMostRequestedAsync();
        Task UpdateOrderAsync(UpdateOrderDto updateOrderDto);
        Task<bool> SoftDeleteOrderAsync(Guid orderId);
    }
}
