using CountrySecure.Application.DTOs.Common;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<PagedResult<Notification>> GetPagedAsync(Guid userId, PageQuery paging, bool unreadOnly);
        Task<int> CountUnreadAsync(Guid userId);

        /// <summary>Marca como leído un aviso del usuario. false si no existe o es de otro usuario.</summary>
        Task<bool> MarkReadAsync(Guid userId, Guid notificationId);
        Task MarkAllReadAsync(Guid userId);
    }
}
