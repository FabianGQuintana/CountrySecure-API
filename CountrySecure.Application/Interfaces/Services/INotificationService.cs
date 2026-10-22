using CountrySecure.Application.DTOs.Common;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<PagedResult<NotificationDto>> GetMineAsync(PageQuery paging, bool unreadOnly);
        Task<int> CountUnreadAsync();
        Task MarkReadAsync(Guid id);
        Task MarkAllReadAsync();
    }
}
