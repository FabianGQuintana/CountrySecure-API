using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private const int MostRequestedCount = 5;

        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto, Guid currentUserId)
        {
            var order = dto.ToEntity();
            order.CreatedBy = currentUserId.ToString();
            order.LastModifiedBy = currentUserId.ToString();

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order.ToResponseDto();
        }

        public async Task<OrderResponseDto> UpdateAsync(Guid id, UpdateOrderDto dto, Guid currentUserId)
        {
            var order = await GetOrThrowAsync(id);

            dto.MapToEntity(order);
            order.Touch(currentUserId.ToString());

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order.ToResponseDto();
        }

        public async Task<OrderResponseDto> DeleteAsync(Guid id, Guid currentUserId)
        {
            var order = await GetOrThrowAsync(id);
            order.SoftDelete(currentUserId.ToString());

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order.ToResponseDto();
        }

        public async Task<OrderResponseDto> RestoreAsync(Guid id, Guid currentUserId)
        {
            var order = await _orderRepository.GetByIdIncludingDeletedAsync(id)
                        ?? throw new KeyNotFoundException($"No se encontró el servicio {id}.");

            order.Restore(currentUserId.ToString());

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order.ToResponseDto();
        }

        public async Task<OrderResponseDto> GetByIdAsync(Guid id, bool includeInactive = false)
        {
            // El Admin lista también los servicios dados de baja y puede abrir su detalle para reactivarlos
            var order = includeInactive
                ? await _orderRepository.GetByIdIncludingDeletedAsync(id)
                : await _orderRepository.GetByIdAsync(id);

            return (order ?? throw new KeyNotFoundException($"No se encontró el servicio {id}.")).ToResponseDto();
        }

        public async Task<PagedResult<OrderResponseDto>> GetPagedAsync(PageQuery paging, OrderCategory? type, string? search, bool includeInactive)
        {
            var page = await _orderRepository.GetPagedAsync(paging, type, search, includeInactive);
            return page.Map(o => o.ToResponseDto());
        }

        public async Task<IReadOnlyList<OrderResponseDto>> GetMostRequestedAsync()
        {
            var orders = await _orderRepository.GetMostRequestedAsync(MostRequestedCount);
            return orders.Select(o => o.ToResponseDto()).ToList();
        }

        private async Task<Order> GetOrThrowAsync(Guid id)
            => await _orderRepository.GetByIdAsync(id)
               ?? throw new KeyNotFoundException($"No se encontró el servicio {id}.");
    }
}
