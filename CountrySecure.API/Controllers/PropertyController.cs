using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.Properties;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ICurrentUserService _currentUser;

        public PropertyController(IPropertyService propertyService, ICurrentUserService currentUser)
        {
            _propertyService = propertyService;
            _currentUser = currentUser;
        }

        // --- CONSULTA ---

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _propertyService.GetAllPropertiesAsync(pageNumber, pageSize));

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _propertyService.GetPropertiesByStatusAsync(PropertyStatus.Available, pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var propertyDto = await _propertyService.GetPropertyByIdAsync(id);
            return propertyDto == null ? NotFound() : Ok(propertyDto);
        }

        // Propiedades del usuario autenticado
        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
            => Ok(await _propertyService.GetPropertiesByOwnerId(_currentUser.GetRequiredUserId()));

        [HttpGet("owner/{ownerId:guid}")]
        public async Task<IActionResult> GetByOwner(Guid ownerId)
            => Ok(await _propertyService.GetPropertiesByOwnerId(ownerId));

        [HttpGet("lot/{lotId:guid}")]
        public async Task<IActionResult> GetByLot(Guid lotId)
            => Ok(await _propertyService.GetPropertiesByLotIdAsync(lotId));

        // --- ESCRITURA (solo Admin) ---

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Post([FromBody] CreatePropertyDto dto)
        {
            var propertyDto = await _propertyService.AddNewPropertyAsync(dto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { id = propertyDto.PropertyId }, propertyDto);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdatePropertyDto updateDto)
        {
            var updatedProperty = await _propertyService.UpdatePropertyAsync(id, updateDto, _currentUser.GetRequiredUserId())
                                  ?? throw new KeyNotFoundException($"Propiedad con ID {id} no encontrada.");
            return Ok(updatedProperty);
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> SoftDeleteToggle(Guid id)
        {
            var updatedPropertyDto = await _propertyService.SoftDeleteToggleAsync(id, _currentUser.GetRequiredUserId())
                                     ?? throw new KeyNotFoundException($"Property with ID {id} not found.");

            var action = updatedPropertyDto.StatusAuditoria == "Active" ? "reactivated" : "deactivated";
            return Ok(new
            {
                Message = $"The Property with ID {id} has been {action} successfully.",
                Property = updatedPropertyDto
            });
        }
    }
}
