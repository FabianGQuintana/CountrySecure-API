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
        /// <summary>Con <paramref name="includeInactive"/> (solo Admin) también devuelve los dados de baja.</summary>
        Task<OrderResponseDto> GetByIdAsync(Guid id, bool includeInactive = false);
        Task<PagedResult<OrderResponseDto>> GetPagedAsync(PageQuery paging, OrderCategory? type, string? search, bool includeInactive);
        Task<IReadOnlyList<OrderResponseDto>> GetMostRequestedAsync();
    }
}
