using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUser;

        public OrderController(IOrderService orderService, ICurrentUserService currentUser)
        {
            _orderService = orderService;
            _currentUser = currentUser;
        }

        // --- ESCRITURA (solo Admin) ---

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var created = await _orderService.CreateOrderAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderDto dto)
            => Ok(await _orderService.UpdateOrderAsync(id, dto, _currentUser.GetRequiredUserId()));

        [HttpPatch("{id:guid}/SoftDelete")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> SoftDeleteOrder(Guid id)
        {
            var updatedOrderDto = await _orderService.SoftDeleteOrderAsync(id, _currentUser.GetRequiredUserId())
                                  ?? throw new KeyNotFoundException($"Orden con ID {id} no encontrada.");

            var action = updatedOrderDto.Status == "Active" ? "reactivado" : "desactivado";
            return Ok(new
            {
                Message = $"La Orden con ID {id} ha sido {action} exitosamente.",
                Order = updatedOrderDto
            });
        }

        // --- CONSULTA ---

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 100)
            => Ok(await _orderService.GetAllOrdersAsync(page, size));

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _orderService.GetAllOrdersWithoutFilterAsync());

        [HttpGet("type/{orderType}")]
        public async Task<IActionResult> GetByType(OrderStatus orderType)
            => Ok(await _orderService.GetByTypeAsync(orderType));

        [HttpGet("supplier/{name}")]
        public async Task<IActionResult> GetBySupplier(string name)
            => Ok(await _orderService.GetBySupplierAsync(name));

        [HttpGet("most-requested")]
        public async Task<IActionResult> GetMostRequested()
            => Ok(await _orderService.GetMostRequestedAsync());
    }
}
