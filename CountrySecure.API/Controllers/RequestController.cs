using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            
            var result = await _requestService.CreateRequestAsync(createRequestDto);
            return Ok(result);
            
            
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

        // GET: api/requests?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestResponseDto>>> GetAllRequests([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var requests = await _requestService.GetAllRequestsAsync(pageNumber, pageSize);

            return Ok(requests);
        }

        // GET: api/requests/status/{status}?pageNumber=1&pageSize=10
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
        [HttpPut("{id}")]
        public async Task<ActionResult<RequestResponseDto>> UpdateRequest(Guid id, [FromBody] UpdateRequestDto updateRequestDto)
        {
            var result = await _requestService.UpdateRequestAsync(id, updateRequestDto);
            return Ok(result);
        }


        
        [HttpPatch("{id}/soft-delete")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var updatedRequest = await _requestService.ToggleActiveAsync(id);

            if (updatedRequest == null)
            {
                return NotFound(new { message = $"Request with id {id} not found." });
            }

            // Devolvemos el DTO de respuesta actualizado
            return Ok(updatedRequest);
        }
    }
}