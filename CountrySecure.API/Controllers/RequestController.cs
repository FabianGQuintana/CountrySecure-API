using CountrySecure.Application.DTOs.Request;
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
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        // POST: api/requests
        [HttpPost]
        public async Task<ActionResult<CreateRequestDto>> CreateRequest([FromBody] CreateRequestDto createRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Get User ID (Auditoría)
                Guid? currentUserId = GetCurrentUserId();

                if (!currentUserId.HasValue)
                {
                    return Unauthorized(); // 401 Unauthorized
                }

                // 2. Call Service (Asegúrate que CreateRequestAsync reciba currentUserId.Value)
                // **Nota:** Si tu CreateRequestAsync en el servicio aún no recibe el Guid, 
                // deberás actualizar su firma para pasarlo.
                var result = await _requestService.CreateRequestAsync(createRequestDto, currentUserId.Value);

                // 3. 201 Created (Usamos Ok(200) o CreatedAtAction si tienes un método GetById)
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException ex)
            {
                // Captura si una FK (User/Order) es inválida. Mensaje en inglés.
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred while creating the request.", detail = ex.Message });
            }
        }

        // GET: api/requests/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestResponseDto>> GetRequestById(Guid id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            return Ok(request);
        }

        // GET: api/requests?pageNumber=1&pageSize=100
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetAllRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var requests = await _requestService.GetAllRequestsAsync(pageNumber, pageSize);

            return Ok(requests);
        }

        // GET: api/requests/status/{status}?pageNumber=1&pageSize=100
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetRequestsByStatus(RequestStatus status, int pageNumber = 1, int pageSize = 100)
        {
            var requests = await _requestService.GetRequestsByStatusAsync(status, pageNumber, pageSize);
            return Ok(requests);
        }

        // GET: api/requests/count/{status}
        [HttpGet("count/{status}")]
        public async Task<ActionResult<int>> CountRequestsByStatus(RequestStatus status)
        {
            var count = await _requestService.CountRequestsByStatusAsync(status);
            return Ok(count);
        }

        // PUT: api/requests/{id}
        [HttpPut("{id:guid}")] // Añadir restricción :guid para claridad
        public async Task<ActionResult<RequestResponseDto>> UpdateRequest(Guid id, [FromBody] UpdateRequestDto updateRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Get User ID (Auditoría)
                Guid? currentUserId = GetCurrentUserId();

                if (!currentUserId.HasValue)
                {
                    return Unauthorized(); // 401 Unauthorized
                }

                // 2. Call Service (Asegúrate que UpdateRequestAsync reciba currentUserId.Value)
                // **Nota:** Al igual que en POST, si tu UpdateRequestAsync aún no recibe el Guid, 
                // deberás actualizar su firma en el servicio y la interfaz.
                var updatedRequest = await _requestService.UpdateRequestAsync(id, updateRequestDto, currentUserId.Value);

                // 3. Return 200 OK and updated DTO
                return Ok(updatedRequest);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Request with ID {id} not found." });
            }
            catch (UnauthorizedAccessException)
            {
                // Captura si el usuario no tiene permisos (ej. no es el creador)
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred while updating the request.", detail = ex.Message });
            }
        }



        // --- SOFT DELETE TOGGLE ---

        [HttpPatch("{id:guid}/SoftDelete")] // Endpoint estandarizado
        public async Task<IActionResult> SoftDeleteToggle(Guid id) // Nombre estandarizado
        {
            try
            {
                // 1. Get User ID (will throw UnauthorizedAccessException if invalid)
                var currentUserId = GetCurrentUserId();

                // 2. Call standardized service method
                var updatedRequest = await _requestService.SoftDeleteToggleAsync(id, currentUserId);

                if (updatedRequest == null)
                {
                    // Mensaje en Inglés
                    return NotFound(new { message = $"Request with ID {id} not found." });
                }

                // 3. Return 200 OK and updated DTO
                // Asumo que RequestResponseDto tiene una propiedad Status (string) de la BaseEntity.
                var action = updatedRequest.BaseEntityStatus == "Active" ? "reactivated" : "deactivated";

                return Ok(new
                {
                    Message = $"The Request with ID {id} has been {action} successfully.",
                    Request = updatedRequest // Returns the full updated DTO
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred while changing request status.", detail = ex.Message });
            }
        }

        // --- UTILITY: Get Current User ID ---
        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid currentUserId))
                throw new UnauthorizedAccessException("User ID not found or is invalid.");

            return currentUserId;
        }
    }
}