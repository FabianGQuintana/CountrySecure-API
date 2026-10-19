using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/properties")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ICurrentUserService _currentUser;

        public PropertyController(IPropertyService propertyService, ICurrentUserService currentUser)
        {
            _propertyService = propertyService;
            _currentUser = currentUser;
        }

        /// <summary>Propiedades del usuario autenticado.</summary>
        [HttpGet("me")]
        public async Task<ActionResult<PagedResult<PropertyResponseDto>>> GetMine([FromQuery] PageQuery paging)
            => Ok(await _propertyService.GetPagedAsync(paging, null, null, _currentUser.GetRequiredUserId(), null, false));

        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PagedResult<PropertyResponseDto>>> GetAll(
            [FromQuery] PageQuery paging,
            [FromQuery] PropertyStatus? status = null,
            [FromQuery] Guid? lotId = null,
            [FromQuery] Guid? ownerId = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includeInactive = false)
            => Ok(await _propertyService.GetPagedAsync(paging, status, lotId, ownerId, search, includeInactive));

        [HttpGet("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PropertyResponseDto>> GetById(Guid id)
            => Ok(await _propertyService.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PropertyResponseDto>> Create([FromBody] CreatePropertyDto dto)
        {
            var created = await _propertyService.CreateAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.PropertyId }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PropertyResponseDto>> Update(Guid id, [FromBody] UpdatePropertyDto dto)
            => Ok(await _propertyService.UpdateAsync(id, dto, _currentUser.GetRequiredUserId()));

        /// <summary>Baja lógica.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PropertyResponseDto>> Delete(Guid id)
            => Ok(await _propertyService.DeleteAsync(id, _currentUser.GetRequiredUserId()));

        [HttpPost("{id:guid}/restore")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<ActionResult<PropertyResponseDto>> Restore(Guid id)
            => Ok(await _propertyService.RestoreAsync(id, _currentUser.GetRequiredUserId()));
    }
}
