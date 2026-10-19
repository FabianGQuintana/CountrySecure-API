using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Servicios (proveedores) de la comunidad.</summary>
    [ApiController]
    [Authorize]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrentUserService _currentUser;

        public OrderController(IOrderService orderService, ICurrentUserService currentUser)
        {
            _orderService = orderService;
            _currentUser = currentUser;
        }

        /// <summary>Lista paginada. Solo un Admin puede pedir los inactivos.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] OrderCategory? type = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _orderService.GetPagedAsync(paging, type, search, includeInactive && _currentUser.IsAdmin));

        [HttpGet("most-requested")]
        public async Task<ActionResult<IReadOnlyList<OrderResponseDto>>> GetMostRequested()
            => Ok(await _orderService.GetMostRequestedAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
            => Ok(await _orderService.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderDto dto)
        {
            var created = await _orderService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<OrderResponseDto>> Update(Guid id, [FromBody] UpdateOrderDto dto)
            => Ok(await _orderService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Baja lógica.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<OrderResponseDto>> Delete(Guid id)
            => Ok(await _orderService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<OrderResponseDto>> Restore(Guid id)
            => Ok(await _orderService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
