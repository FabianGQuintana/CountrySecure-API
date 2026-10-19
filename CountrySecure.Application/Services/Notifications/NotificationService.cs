using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifications;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public NotificationService(INotificationRepository notifications, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _notifications = notifications;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<NotificationDto>> GetMineAsync(PageQuery paging, bool unreadOnly)
        {
            var page = await _notifications.GetPagedAsync(_currentUser.GetRequiredUserId(), paging, unreadOnly);
            return page.Map(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                PermissionId = n.PermissionId,
                CreatedAt = n.CreatedAt,
                Read = n.ReadAt != null
            });
        }

        public Task<int> CountUnreadAsync() => _notifications.CountUnreadAsync(_currentUser.GetRequiredUserId());

        public async Task MarkReadAsync(Guid id)
        {
            if (!await _notifications.MarkReadAsync(_currentUser.GetRequiredUserId(), id))
                throw new KeyNotFoundException("Notification not found.");
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync()
        {
            await _notifications.MarkAllReadAsync(_currentUser.GetRequiredUserId());
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
