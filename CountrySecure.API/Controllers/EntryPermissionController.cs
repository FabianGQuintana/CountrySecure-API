using CountrySecure.API.Authorization;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EntryPermissionsController : ControllerBase
    {
        private readonly IEntryPermissionService _entryPermissionService;
        private readonly ICurrentUserService _currentUser;

        public EntryPermissionsController(IEntryPermissionService entryPermissionService, ICurrentUserService currentUser)
        {
            _entryPermissionService = entryPermissionService;
            _currentUser = currentUser;
        }

        // -------------------------------------------------------------------
        // CRUD
        // -------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEntryPermissionDto createDto)
        {
            var result = await _entryPermissionService.AddNewEntryPermissionAsync(createDto, _currentUser.GetRequiredUserId());
            return CreatedAtAction(nameof(GetById), new { entryPermissionId = result.Id }, result);
        }

        [HttpGet("{entryPermissionId}")]
        public async Task<IActionResult> GetById(Guid entryPermissionId)
        {
            var result = await _entryPermissionService.GetEntryPermissionByIdAsync(entryPermissionId)
                         ?? throw new KeyNotFoundException($"EntryPermission with ID {entryPermissionId} not found.");
            return Ok(result);
        }

        [HttpPut("{entryPermissionId}")]
        public async Task<IActionResult> Update(Guid entryPermissionId, [FromBody] UpdateEntryPermissionDto updateDto)
        {
            var result = await _entryPermissionService.UpdateEntryPermissionAsync(updateDto, entryPermissionId, _currentUser.GetRequiredUserId())
                         ?? throw new KeyNotFoundException($"EntryPermission with ID {entryPermissionId} not found.");
            return Ok(result);
        }

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            var updatedPermissionDto = await _entryPermissionService.SoftDeleteEntryPermissionAsync(id, _currentUser.GetRequiredUserId())
                                       ?? throw new KeyNotFoundException($"Permiso de Entrada con ID {id} no encontrado.");

            var action = updatedPermissionDto.BaseEntityStatus == "Active" ? "reactivado" : "desactivado";
            return Ok(new
            {
                Message = $"El Permiso de Entrada con ID {id} ha sido {action} exitosamente.",
                Permission = updatedPermissionDto
            });
        }

        // -------------------------------------------------------------------
        // CONSULTAS DE COLECCIÓN
        // -------------------------------------------------------------------

        [HttpGet]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _entryPermissionService.GetAllEntryPermissionsAsync(pageNumber, pageSize));

        [HttpGet("type/{permissionType}")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> GetByType(PermissionType permissionType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
            => Ok(await _entryPermissionService.GetEntryPermissionsByTypeAsync(permissionType, pageNumber, pageSize));

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            // Un residente solo puede consultar sus propios permisos
            if (_currentUser.Role == Domain.Constants.RoleTypes.Resident && _currentUser.UserId != userId)
                throw new UnauthorizedAccessException("You can only view your own entry permissions.");

            return Ok(await _entryPermissionService.GetEntryPermissionsByUserIdAsync(userId));
        }

        [HttpGet("visit/{visitId}")]
        public async Task<IActionResult> GetByVisitId(Guid visitId)
            => Ok(await _entryPermissionService.GetEntryPermissionsByVisitIdAsync(visitId));

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetByServiceId(Guid serviceId)
            => Ok(await _entryPermissionService.GetEntryPermissionsByServiceIdAsync(serviceId));

        // -------------------------------------------------------------------
        // OPERACIONES DE PORTERÍA (Security / Admin)
        // -------------------------------------------------------------------

        [HttpGet("validate")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> ValidateQrCode([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("QR code value is required.");

            return Ok(await _entryPermissionService.ValidateQrCodeAsync(code));
        }

        [HttpGet("today")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> GetTodayPermissions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            // Día local del servidor convertido a UTC (PostgreSQL timestamptz)
            var todayUtc = DateTime.Today.ToUniversalTime();
            return Ok(await _entryPermissionService.GetActivePermissionsForDateAsync(todayUtc, pageNumber, pageSize));
        }

        [HttpPatch("{permissionId:guid}/checkin")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> RegisterCheckIn(Guid permissionId)
        {
            var updatedPermission = await _entryPermissionService.RegisterCheckInAsync(permissionId, _currentUser.GetRequiredUserId());
            return Ok(new { Message = "Entrada registrada exitosamente.", Permission = updatedPermission });
        }

        [HttpPatch("{permissionId:guid}/checkout")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> RegisterCheckOut(Guid permissionId)
        {
            var updatedPermission = await _entryPermissionService.RegisterCheckOutAsync(permissionId, _currentUser.GetRequiredUserId());
            return Ok(new { Message = "Salida registrada exitosamente.", Permission = updatedPermission });
        }

        [HttpGet("history")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> GetEntryHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? type = null)
            => Ok(await _entryPermissionService.GetEntryLogsAsync(pageNumber, pageSize, search, type));

        [HttpPost("register-movement")]
        [Authorize(Policy = Policies.SecurityStaff)]
        public async Task<IActionResult> RegisterMovement([FromBody] RegisterMovementDto dto)
        {
            // El guardia se toma del token, nunca del body.
            var guardId = _currentUser.GetRequiredUserId();

            if (dto.IsEntry)
            {
                await _entryPermissionService.RegisterCheckInAsync(dto.PermissionId, guardId);
                return Ok(new { Message = "Entrada registrada con éxito." });
            }

            await _entryPermissionService.RegisterCheckOutAsync(dto.PermissionId, guardId);
            return Ok(new { Message = "Salida registrada con éxito." });
        }
    }
}
