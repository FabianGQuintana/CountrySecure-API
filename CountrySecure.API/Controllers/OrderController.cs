using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.DTOs.Visits;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Visits;
using CountrySecure.Domain.Enums;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CountrySecure.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("/ping-order")]
        public IActionResult Ping()
        {
            return Ok("OrderController is working! 🏓");
        }


        /*
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userId = GetCurrentUserId();

            var created = await _orderService.CreateOrderAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        */

        [HttpPost]
        public async Task<IActionResult> AddNewOrderAsync([FromBody] CreateOrderDto newOrderDto)
        {    
            var userId = Guid.Empty;

            var createdOrder = await _orderService.CreateOrderAsync(newOrderDto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdOrder.Id },
                createdOrder
            );
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound("Order not found.");

            return Ok(order);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var results = await _orderService.GetAllOrdersAsync(page, size);

            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _orderService.GetAllOrdersWithoutFilterAsync();

            return Ok(results);
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetByStatus([FromQuery] bool isActive)
        {
            var results = await _orderService.GetByStatusAsync(isActive);

            return Ok(results);
        }

        [HttpGet("type/{orderType}")]
        public async Task<IActionResult> GetByType(OrderStatus orderType)
        {
            var results = await _orderService.GetByTypeAsync(orderType);

            return Ok(results);
        }

        [HttpGet("supplier/{name}")]
        public async Task<IActionResult> GetBySupplier(string name)
        {
            var results = await _orderService.GetBySupplierAsync(name);

            return Ok(results);
        }

        [HttpGet("most-requested")]
        public async Task<IActionResult> GetMostRequested()
        {
            var results = await _orderService.GetMostRequestedAsync();

            return Ok(results);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id mismatch.");

            await _orderService.UpdateOrderAsync(dto);

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return Guid.Parse(userId);
        }
    }
}
