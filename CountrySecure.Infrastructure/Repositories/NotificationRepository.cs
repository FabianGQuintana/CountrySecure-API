using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        public Task<PagedResult<Notification>> GetPagedAsync(Guid userId, PageQuery paging, bool unreadOnly)
        {
            var query = Set.AsNoTracking().Where(n => n.UserId == userId);
            if (unreadOnly) query = query.Where(n => n.ReadAt == null);
            return query.OrderByDescending(n => n.CreatedAt).ToPagedResultAsync(paging);
        }

        public Task<int> CountUnreadAsync(Guid userId)
            => Set.CountAsync(n => n.UserId == userId && n.ReadAt == null);

        public async Task<bool> MarkReadAsync(Guid userId, Guid notificationId)
        {
            var notification = await Set.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification == null) return false;
            notification.ReadAt ??= DateTime.UtcNow;
            return true;
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            var unread = await Set.Where(n => n.UserId == userId && n.ReadAt == null).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var n in unread) n.ReadAt = now;
        }
    }
}
