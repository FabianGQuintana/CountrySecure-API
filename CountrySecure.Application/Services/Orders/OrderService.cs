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
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 1. Crear Orden
        // ============================================================
        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto newOrderDto, Guid currentUserId)
        {
            var newOrder = newOrderDto.ToEntity();

            // Auditoría
            newOrder.CreatedBy = currentUserId.ToString();

            newOrder.LastModifiedAt = DateTime.UtcNow; // Asignamos fecha
            newOrder.LastModifiedBy = currentUserId.ToString();

            await _orderRepository.AddAsync(newOrder);
            await _unitOfWork.SaveChangesAsync();

            return newOrder.ToResponseDto();
        }

        // ============================================================
        // 2. Obtener por Id
        // ============================================================
        public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null || order.IsDeleted)
                return null;

            return order.ToResponseDto();
        }

        // ============================================================
        // 3. Obtener todos con paginación
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var allOrders = await _orderRepository.GetAllAsync();

            var paged = allOrders
                .Where(o => !o.IsDeleted)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return paged.ToResponseDto();
        }

        // ============================================================
        // 4. Obtener todos sin filtros
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersWithoutFilterAsync()
        {
            var all = await _orderRepository.GetAllAsync();

            return all
                .Where(o => !o.IsDeleted)
                .ToResponseDto();
        }

        // ============================================================
        // 5. Obtener por estado
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetByStatusAsync(bool isActive)
        {
            var results = await _orderRepository.GetByStatusAsync(isActive);

            return results
                .Where(o => !o.IsDeleted)
                .ToResponseDto();
        }

        // ============================================================
        // 6. Obtener por tipo
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetByTypeAsync(OrderStatus orderType)
        {
            var results = await _orderRepository.GetByTypeAsync(orderType);

            return results
                .Where(o => !o.IsDeleted)
                .ToResponseDto();
        }

        // ============================================================
        // 7. Obtener por proveedor
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetBySupplierAsync(string supplierName)
        {
            var results = await _orderRepository.GetBySupplierAsync(supplierName);

            return results
                .Where(o => !o.IsDeleted)
                .ToResponseDto();
        }

        // ============================================================
        // 8. Los más solicitados
        // ============================================================
        public async Task<IEnumerable<OrderResponseDto>> GetMostRequestedAsync()
        {
            var results = await _orderRepository.GetMostRequestedAsync();

            return results
                .Where(o => !o.IsDeleted)
                .ToResponseDto();
        }
        // ============================================================
        // 9. Actualizar Orden
        // ============================================================
        public async Task UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrderDto, Guid currentUserId)
        {
            var existing = await _orderRepository.GetByIdAsync(orderId);

            if (existing == null || existing.IsDeleted)
                throw new KeyNotFoundException("Order not found."); 


            updateOrderDto.MapToEntity(existing);

            existing.LastModifiedAt = DateTime.UtcNow;
            existing.LastModifiedBy = currentUserId.ToString(); 
                                                               

            await _orderRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();
        }

        // ============================================================
        // 10. Soft Delete
        // ============================================================
        public async Task<bool> SoftDeleteOrderAsync(Guid orderId, Guid currentUserId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);

            if (order == null || order.IsDeleted)
                return false;

            order.Status = "Inactive";
            order.DeletedAt = DateTime.UtcNow;
            order.LastModifiedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
