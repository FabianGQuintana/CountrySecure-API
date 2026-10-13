using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly ICurrentUserService _currentUser;

        public RequestsController(IRequestService requestService, ICurrentUserService currentUser)
        {
            _requestService = requestService;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto createRequestDto)
            => Ok(await _requestService.CreateRequestAsync(createRequestDto, _currentUser.GetRequiredUserId()));

        // Solicitudes del usuario autenticado
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
            => Ok(await _requestService.GetRequestsByUserIdAsync(_currentUser.GetRequiredUserId()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RequestResponseDto>> GetRequestById(Guid id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            return request == null ? NotFound() : Ok(request);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetAllRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _requestService.GetAllRequestsAsync(pageNumber, pageSize));

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetRequestsByStatus(RequestStatus status, int pageNumber = 1, int pageSize = 100)
            => Ok(await _requestService.GetRequestsByStatusAsync(status, pageNumber, pageSize));

        [HttpGet("count/{status}")]
        public async Task<ActionResult<int>> CountRequestsByStatus(RequestStatus status)
            => Ok(await _requestService.CountRequestsByStatusAsync(status));

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RequestResponseDto>> UpdateRequest(Guid id, [FromBody] UpdateRequestDto updateRequestDto)
            => Ok(await _requestService.UpdateRequestAsync(id, updateRequestDto, _currentUser.GetRequiredUserId()));

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDeleteToggle(Guid id)
        {
            var updatedRequest = await _requestService.SoftDeleteToggleAsync(id, _currentUser.GetRequiredUserId())
                                 ?? throw new KeyNotFoundException($"Request with ID {id} not found.");

            var action = updatedRequest.BaseEntityStatus == "Active" ? "reactivated" : "deactivated";
            return Ok(new
            {
                Message = $"The Request with ID {id} has been {action} successfully.",
                Request = updatedRequest
            });
        }
    }
}
