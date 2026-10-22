using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Amenities;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/amenities")]
    public class AmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly ICurrentUserService _currentUser;

        public AmenityController(IAmenityService amenityService, ICurrentUserService currentUser)
        {
            _amenityService = amenityService;
            _currentUser = currentUser;
        }

        /// <summary>Lista paginada de amenities. Solo un Admin puede pedir los inactivos.</summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<AmenityResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] string? search = null,
            [FromQuery] int? minCapacity = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _amenityService.GetPagedAsync(paging, search, minCapacity, includeInactive && _currentUser.IsAdmin));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AmenityResponseDto>> GetById(Guid id)
            => Ok(await _amenityService.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<AmenityResponseDto>> Create([FromBody] AmenityCreateDto dto)
        {
            var created = await _amenityService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<AmenityResponseDto>> Update(Guid id, [FromBody] AmenityUpdateDto dto)
            => Ok(await _amenityService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Baja lógica.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<AmenityResponseDto>> Delete(Guid id)
            => Ok(await _amenityService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<AmenityResponseDto>> Restore(Guid id)
            => Ok(await _amenityService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
