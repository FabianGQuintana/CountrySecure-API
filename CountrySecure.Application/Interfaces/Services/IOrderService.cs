using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateAsync(CreateOrderDto dto, Guid currentUserId);
        Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto, Guid currentUserId);
        Task<OrderResponseDto> DeleteAsync(Guid id, Guid currentUserId);
        Task<OrderResponseDto> RestoreAsync(Guid id, Guid currentUserId);
        Task<OrderResponseDto> GetByIdAsync(Guid id);
        Task<PagedResult<OrderResponseDto>> GetPagedAsync(PageQuery paging, OrderCategory? type, string? search, bool includeInactive);
        Task<IReadOnlyList<OrderResponseDto>> GetMostRequestedAsync();
    }
}
