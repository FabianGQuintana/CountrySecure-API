using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CountrySecure.API.Controllers
{
    /// <summary>Avisos del usuario autenticado (p. ej. "Tu visita ingresó al barrio").</summary>
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetMine([FromQuery] PageQuery paging, [FromQuery] bool unreadOnly = false)
            => Ok(await _service.GetMineAsync(paging, unreadOnly));

        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountDto>> UnreadCount()
            => Ok(new UnreadCountDto { Count = await _service.CountUnreadAsync() });

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _service.MarkReadAsync(id);
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _service.MarkAllReadAsync();
            return NoContent();
        }
    }
}
