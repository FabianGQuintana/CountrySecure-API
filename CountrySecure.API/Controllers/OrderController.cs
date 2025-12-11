using CountrySecure.Application.DTOs.Order;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE ESCRITURA (POST, PUT, DELETE)
        // -------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            try
            {
                // Obtener el usuario autenticado (Lanza UnauthorizedAccessException si falla)
                var userId = GetCurrentUserId();

                var created = await _orderService.CreateOrderAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id }, // Usar 'id' para coincidir con la ruta GetById
                    created
                );
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(); // 401 Unauthorized
            }
            catch (Exception ex)
            {
                // Capturar errores de servicio/base de datos
                return StatusCode(500, new { message = "Error interno al crear la orden.", detail = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request
            }

            try
            {
                var currentUserId = GetCurrentUserId();

                // Capturamos el DTO de respuesta devuelto por el servicio
                var updatedOrderDto = await _orderService.UpdateOrderAsync(id, dto, currentUserId);

                //  Devolver 200 OK con el cuerpo de la orden actualizada.
                return Ok(updatedOrderDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // 404 Not Found (Usamos el mensaje de la excepción)
            }
            catch (UnauthorizedAccessException)
            {
                // Si GetCurrentUserId() lanza UnauthorizedAccessException
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al actualizar la orden.", detail = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        [Authorize]
        public async Task<IActionResult> SoftDeleteOrder(Guid id)
        {
            // 1. Obtener el ID del usuario actual
            Guid? currentUserId = GetCurrentUserId(); 
            if (!currentUserId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                // 2. Llamada al servicio que ahora devuelve el DTO o null
                var updatedOrderDto = await _orderService.SoftDeleteOrderAsync(id, currentUserId.Value);

                if (updatedOrderDto == null)
                {
                    return NotFound($"Orden con ID {id} no encontrada."); // 404 Not Found
                }

                // 3. Retorno de éxito (200 OK y el DTO actualizado)
                var action = updatedOrderDto.Status == "Active" ? "reactivado" : "desactivado";

                return Ok(new
                {
                    Message = $"La Orden con ID {id} ha sido {action} exitosamente.",
                    Order = updatedOrderDto // Devuelve el DTO completo y actualizado
                });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado durante el cambio de estado de la Orden.");
            }
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE LECTURA (GET)
        // -------------------------------------------------------------------

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                    return NotFound($"Orden con ID {id} no encontrada.");

                return Ok(order); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al buscar la orden.", detail = ex.Message });
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int size = 100)
        {
            try
            {
                var results = await _orderService.GetAllOrdersAsync(page, size);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al paginar órdenes.", detail = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var results = await _orderService.GetAllOrdersWithoutFilterAsync();
                return Ok(results); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al obtener todas las órdenes.", detail = ex.Message });
            }
        }

       

        [HttpGet("type/{orderType}")]
        public async Task<IActionResult> GetByType(OrderStatus orderType)
        {
            try
            {
                var results = await _orderService.GetByTypeAsync(orderType);
                return Ok(results); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al buscar órdenes por tipo.", detail = ex.Message });
            }
        }

        // Si busco uno eliminado me devuelve el 200 pero no el vuerpo
        [HttpGet("supplier/{name}")]
        public async Task<IActionResult> GetBySupplier(string name)
        {
            try
            {
                var results = await _orderService.GetBySupplierAsync(name);
                return Ok(results); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al buscar órdenes por proveedor.", detail = ex.Message });
            }
        }

        [HttpGet("most-requested")]
        public async Task<IActionResult> GetMostRequested()
        {
            try
            {
                var results = await _orderService.GetMostRequestedAsync();
                return Ok(results); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno al buscar las órdenes más solicitadas.", detail = ex.Message });
            }
        }

        // -------------------------------------------------------------------
        // MÉTODO DE UTILIDAD
        // -------------------------------------------------------------------

        // Este método ahora lanza una excepción que será atrapada por los try/catch de arriba
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token."); // Lanza una excepción específica

            return Guid.Parse(userId);
        }
    }
}