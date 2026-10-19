using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Solicitudes de servicio de los residentes. Un residente solo accede a las propias.</summary>
    [ApiController]
    [Authorize(Roles = $"{RoleTypes.Admin},{RoleTypes.Resident}")]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly ICurrentUserService _currentUser;

        public RequestsController(IRequestService requestService, ICurrentUserService currentUser)
        {
            _requestService = requestService;
            _currentUser = currentUser;
        }

        /// <summary>Solicitudes del usuario autenticado.</summary>
        [HttpGet("me")]
        public async Task<ActionResult<PagedResult<RequestResponseDto>>> GetMine(
            [FromQuery] PageQuery paging, [FromQuery] RequestStatus? status = null)
            => Ok(await _requestService.GetPagedAsync(paging, status, onlyMine: true, includeInactive: false));

        [HttpGet]
        public async Task<ActionResult<PagedResult<RequestResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] RequestStatus? status = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _requestService.GetPagedAsync(paging, status, onlyMine: false, includeInactive));

        [HttpGet("count")]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<ActionResult<int>> Count([FromQuery] RequestStatus status)
            => Ok(await _requestService.CountByStatusAsync(status));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RequestResponseDto>> GetById(Guid id)
            => Ok(await _requestService.GetByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<RequestResponseDto>> Create([FromBody] CreateRequestDto dto)
        {
            var created = await _requestService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RequestResponseDto>> Update(Guid id, [FromBody] UpdateRequestDto dto)
            => Ok(await _requestService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Cancela la solicitud (baja lógica).</summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<RequestResponseDto>> Delete(Guid id)
            => Ok(await _requestService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Roles = RoleTypes.Admin)]
        public async Task<ActionResult<RequestResponseDto>> Restore(Guid id)
            => Ok(await _requestService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
