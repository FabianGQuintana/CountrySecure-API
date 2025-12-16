using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims; 
using System.Threading.Tasks;

namespace CountrySecure.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntryPermissionsController : ControllerBase
    {
        private readonly IEntryPermissionService _entryPermissionService;

        public EntryPermissionsController(IEntryPermissionService entryPermissionService)
        {
            _entryPermissionService = entryPermissionService;
        }

        // --- Método Auxiliar para Seguridad (Usando Claims) ---
        private Guid GetCurrentUserId()
        {
            // Busca el ClaimTypes.NameIdentifier (sub) o el claim que uses para almacenar el ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si el claim existe y es convertible a Guid, lo devuelve. Si no, devuelve Guid.Empty
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }
            return Guid.Empty; // Guid.Empty indica que el ID no pudo ser recuperado
        }

        // -------------------------------------------------------------------
        // POST: Creación de un nuevo permiso (201 Created)
        // -------------------------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEntryPermissionDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                // Validar que el usuario fue autenticado
                if (currentUserId == Guid.Empty) return Unauthorized();

                var result = await _entryPermissionService.AddNewEntryPermissionAsync(createDto, currentUserId);

                return CreatedAtAction(nameof(GetById), new { entryPermissionId = result.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Obtener por ID (200 OK / 404 Not Found)
        // -------------------------------------------------------------------

        [HttpGet("{entryPermissionId}")]
        public async Task<IActionResult> GetById(Guid entryPermissionId)
        {
            var result = await _entryPermissionService.GetEntryPermissionByIdAsync(entryPermissionId);

            if (result == null)
            {
                return NotFound($"EntryPermission with ID {entryPermissionId} not found.");
            }

            return Ok(result);
        }

        // -------------------------------------------------------------------
        // PUT: Actualización total o parcial (200 OK / 404 Not Found)
        // -------------------------------------------------------------------

        [HttpPut("{entryPermissionId}")]
        public async Task<IActionResult> Update(Guid entryPermissionId, [FromBody] UpdateEntryPermissionDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty) return Unauthorized(); // Seguridad

                // CORREGIDO: Pasamos el currentUserId para la lógica de auditoría/autorización en el servicio
                var result = await _entryPermissionService.UpdateEntryPermissionAsync(updateDto, entryPermissionId, currentUserId);

                if (result == null)
                {
                    return NotFound($"EntryPermission with ID {entryPermissionId} not found.");
                }

                return Ok(result); // 200 OK
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // DELETE: Eliminación Lógica 
        // -------------------------------------------------------------------

        [HttpPatch("{id:guid}/SoftDelete")]
        public async Task<IActionResult> SoftDelete(Guid id)
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
                var updatedPermissionDto = await _entryPermissionService.SoftDeleteEntryPermissionAsync(id, currentUserId.Value);

                if (updatedPermissionDto == null)
                {
                    return NotFound($"Permiso de Entrada con ID {id} no encontrado."); // 404 Not Found
                }

                // 3. Retorno de éxito (200 OK y el DTO actualizado)
                var action = updatedPermissionDto.BaseEntityStatus == "Active" ? "reactivado" : "desactivado";

                return Ok(new
                {
                    Message = $"El Permiso de Entrada con ID {id} ha sido {action} exitosamente.",
                    Permission = updatedPermissionDto // Devuelve el DTO completo y actualizado
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error inesperado durante el cambio de estado del Permiso de Entrada.");
            }
        }

        // -------------------------------------------------------------------
        // GET: Consultas de Colección (Paginated/Filtered)
        // -------------------------------------------------------------------

        [HttpGet] // GET /api/entrypermissions?pageNumber=1&pageSize=100
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var results = await _entryPermissionService.GetAllEntryPermissionsAsync(pageNumber, pageSize);
            return Ok(results);
        }

        [HttpGet("type/{permissionType}")] // GET /api/entrypermissions/type/Visitante
        public async Task<IActionResult> GetByType(PermissionType permissionType, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var results = await _entryPermissionService.GetEntryPermissionsByTypeAsync(permissionType, pageNumber, pageSize);
            return Ok(results);
        }

        // -------------------------------------------------------------------
        // Consultas por FK (UserID, VisitID, ServiceID)
        // -------------------------------------------------------------------

        [HttpGet("user/{userId}")] // GET /api/entrypermissions/user/{userId}
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var results = await _entryPermissionService.GetEntryPermissionsByUserIdAsync(userId);
            return Ok(results); // Devuelve 200 OK con lista vacía si no hay resultados
        }

        [HttpGet("visit/{visitId}")] // GET /api/entrypermissions/visit/{visitId}
        public async Task<IActionResult> GetByVisitId(Guid visitId)
        {
            var results = await _entryPermissionService.GetEntryPermissionsByVisitIdAsync(visitId);
            return Ok(results);
        }

        [HttpGet("service/{serviceId}")] // GET /api/entrypermissions/service/{serviceId}
        public async Task<IActionResult> GetByServiceId(Guid serviceId)
        {
            var results = await _entryPermissionService.GetEntryPermissionsByServiceIdAsync(serviceId);
            return Ok(results);
        }

        // -------------------------------------------------------------------
        // VALIDACIÓN DE QR
        // -------------------------------------------------------------------

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateQrCode([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("QR code value is required.");
            }

            try
            {
                // Llama al servicio, que ahora devuelve GateCheckResponseDto
                var validationData = await _entryPermissionService.ValidateQrCodeAsync(code);

                // 200 OK: Devuelve los datos de corroboración (luz verde/roja en el CheckResultStatus)
                return Ok(validationData);
            }
            catch (KeyNotFoundException ex)
            {
                // 404 Not Found (El código nunca existió)
                return NotFound(new { Message = ex.Message, CheckResultStatus = "No Encontrado" });
            }
            catch (Exception ex)
            {
                // En caso de otros errores (ej. la base de datos falla)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Permisos del Día (La consulta que usa el MainView)
        // -------------------------------------------------------------------

        [HttpGet("today")] // GET /api/entrypermissions/today?pageNumber=1&pageSize=10
        public async Task<IActionResult> GetTodayPermissions(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 100) 
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                // Pasa los parámetros al servicio
                var results = await _entryPermissionService.GetActivePermissionsForDateAsync(today, pageNumber, pageSize);

                return Ok(results);
            }
            catch (Exception ex)
            {
                // En un entorno real, es mejor loguear la excepción y devolver un error genérico
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // -------------------------------------------------------------------
        // PATCH: Registro de Entrada (Check-In)
        // -------------------------------------------------------------------

        [HttpPatch("{permissionId:guid}/checkin")]
        public async Task<IActionResult> RegisterCheckIn(Guid permissionId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty) return Unauthorized();

                var updatedPermission = await _entryPermissionService.RegisterCheckInAsync(permissionId, currentUserId);

                return Ok(new
                {
                    Message = "Entrada registrada exitosamente.",
                    Permission = updatedPermission
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Permiso no encontrado.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al registrar la entrada: {ex.Message}");
            }
        }


        // -------------------------------------------------------------------
        // PATCH: Registro de Salida (Check-Out)
        // -------------------------------------------------------------------

        [HttpPatch("{permissionId:guid}/checkout")]
        public async Task<IActionResult> RegisterCheckOut(Guid permissionId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == Guid.Empty) return Unauthorized();

                var updatedPermission = await _entryPermissionService.RegisterCheckOutAsync(permissionId, currentUserId);

                return Ok(new
                {
                    Message = "Salida registrada exitosamente.",
                    Permission = updatedPermission
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Permiso no encontrado.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al registrar la salida: {ex.Message}");
            }
        }

        // -------------------------------------------------------------------
        // GET: Historial de Entradas/Salidas
        // -------------------------------------------------------------------

        [HttpGet("history")]
        public async Task<IActionResult> GetEntryHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null) 
        {
            try
            {
                var results = await _entryPermissionService.GetEntryLogsAsync(pageNumber, pageSize, search);

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("register-movement")]
        public async Task<IActionResult> RegisterMovement([FromBody] RegisterMovementDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (dto.IsEntry)
                {
                    // Llama a la lógica de Check-In
                    await _entryPermissionService.RegisterCheckInAsync(dto.PermissionId, dto.GuardId);
                    return Ok(new { Message = "Entrada registrada con éxito." });
                }
                else
                {
                    // Llama a la lógica de Check-Out
                    await _entryPermissionService.RegisterCheckOutAsync(dto.PermissionId, dto.GuardId);
                    return Ok(new { Message = "Salida registrada con éxito." });
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Maneja casos como "Ya ingresó y salió" o "Entrada no registrada"
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}