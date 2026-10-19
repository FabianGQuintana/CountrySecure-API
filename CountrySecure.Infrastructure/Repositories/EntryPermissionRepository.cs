using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Repositories
{
    public class EntryPermissionRepository : GenericRepository<EntryPermission>, IEntryPermissionRepository
    {
        public EntryPermissionRepository(CountrySecureDbContext dbContext) : base(dbContext) { }

        private IQueryable<EntryPermission> WithDetails(bool includeDeleted = false)
            => Set.IncludeDeleted(includeDeleted)
                  .Include(p => p.User)
                  .Include(p => p.Visit)
                  .Include(p => p.Order)
                  .Include(p => p.CheckInGuard)
                  .Include(p => p.CheckOutGuard);

        public Task<EntryPermission?> GetWithDetailsAsync(Guid id, bool includeDeleted = false)
            => WithDetails(includeDeleted).FirstOrDefaultAsync(p => p.Id == id);

        public Task<EntryPermission?> GetByQrCodeAsync(string qrCodeValue, bool includeDeleted = false)
            => WithDetails(includeDeleted).FirstOrDefaultAsync(p => p.QrCodeValue == qrCodeValue);

        public Task<PagedResult<EntryPermission>> GetPagedAsync(
            PageQuery paging,
            PermissionType? type,
            PermissionStatus? status,
            Guid? userId,
            Guid? visitId,
            Guid? orderId,
            string? search,
            bool includeDeleted)
        {
            var query = WithDetails(includeDeleted).AsNoTracking();

            if (type.HasValue) query = query.Where(p => p.PermissionType == type.Value);
            if (status.HasValue) query = query.Where(p => p.EntryPermissionState == status.Value);
            if (userId.HasValue) query = query.Where(p => p.UserId == userId.Value);
            if (visitId.HasValue) query = query.Where(p => p.VisitId == visitId.Value);
            if (orderId.HasValue) query = query.Where(p => p.OrderId == orderId.Value);
            query = ApplySearch(query, search);

            return query.OrderByDescending(p => p.ValidFrom).ToPagedResultAsync(paging);
        }

        public Task<PagedResult<EntryPermission>> GetTodayAsync(PageQuery paging, DateTime dayStartUtc)
        {
            var dayEndUtc = dayStartUtc.AddDays(1);

            var query = WithDetails().AsNoTracking()
                .Where(p => p.ValidFrom >= dayStartUtc && p.ValidFrom < dayEndUtc)
                .Where(p => p.EntryPermissionState == PermissionStatus.Pending
                            || (p.EntryPermissionState == PermissionStatus.Completed && p.EntryTime.HasValue && !p.DepartureTime.HasValue));

            return query.OrderBy(p => p.ValidFrom).ToPagedResultAsync(paging);
        }

        public Task<PagedResult<EntryPermission>> GetHistoryAsync(PageQuery paging, string? search, PermissionType? type)
        {
            var query = WithDetails().AsNoTracking().Where(p => p.EntryTime.HasValue);

            if (type.HasValue) query = query.Where(p => p.PermissionType == type.Value);
            query = ApplySearch(query, search);

            return query.OrderByDescending(p => p.EntryTime).ToPagedResultAsync(paging);
        }

        // Busca por visitante (nombre, apellido, DNI), residente o proveedor
        private static IQueryable<EntryPermission> ApplySearch(IQueryable<EntryPermission> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            var term = search.Trim().ToLower();
            return query.Where(p =>
                (p.Visit != null && ((p.Visit.NameVisit + " " + p.Visit.LastNameVisit).ToLower().Contains(term)
                                     || p.Visit.DniVisit.ToString().Contains(term)))
                || (p.User != null && (p.User.Name + " " + p.User.Lastname).ToLower().Contains(term))
                || (p.Order != null && p.Order.SupplierName.ToLower().Contains(term)));
        }
    }
}
