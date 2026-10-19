using CountrySecure.Application.DTOs.Stats;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Services
{
    /// <summary>
    /// Consultas de agregación (solo lectura) para los KPIs de los dashboards.
    /// Vive en Infrastructure porque son queries de conteo directas sobre el DbContext.
    /// </summary>
    public class StatsService : IStatsService
    {
        private readonly CountrySecureDbContext _db;

        public StatsService(CountrySecureDbContext db)
        {
            _db = db;
        }

        // "Hoy" según la zona horaria del servidor, expresado en UTC (PostgreSQL timestamptz)
        private static (DateTime start, DateTime end) TodayRangeUtc()
        {
            var start = DateTime.Today.ToUniversalTime();
            return (start, start.AddDays(1));
        }

        public async Task<AdminStatsDto> GetAdminStatsAsync()
        {
            var now = DateTime.UtcNow;
            var (todayStart, todayEnd) = TodayRangeUtc();

            var users = _db.Users.AsNoTracking();
            var turns = _db.Turns.AsNoTracking().Where(t => t.TurnStatus != TurnStatus.Cancelled);
            var permits = _db.EntryPermissions.AsNoTracking();

            return new AdminStatsDto
            {
                TotalResidents = await users.CountAsync(u => u.Role == RoleTypes.Resident),
                TotalSecurityStaff = await users.CountAsync(u => u.Role == RoleTypes.Security),
                ActiveUsers = await users.CountAsync(),
                InactiveUsers = await users.IgnoreQueryFilters().CountAsync(u => u.DeletedAt != null),
                TotalLots = await _db.Lots.AsNoTracking().CountAsync(),
                TotalProperties = await _db.Properties.AsNoTracking().CountAsync(),
                OccupiedProperties = await _db.Properties.AsNoTracking().CountAsync(p => p.UserId != null),
                TotalAmenities = await _db.Amenities.AsNoTracking().CountAsync(),
                UpcomingReservations = await turns.CountAsync(t => t.StartTime >= now),
                ReservationsToday = await turns.CountAsync(t => t.StartTime >= todayStart && t.StartTime < todayEnd),
                PendingRequests = await _db.Requests.AsNoTracking().CountAsync(r => r.RequestStatus == RequestStatus.Pending),
                ActiveOrders = await _db.Orders.AsNoTracking().CountAsync(),
                PermitsToday = await permits.CountAsync(p => p.ValidFrom < todayEnd && (p.ValidTo == null || p.ValidTo >= todayStart)),
                VisitorsInside = await permits.CountAsync(p => p.EntryTime != null && p.DepartureTime == null),
            };
        }

        public async Task<ResidentStatsDto> GetResidentStatsAsync(Guid residentId)
        {
            var now = DateTime.UtcNow;

            var myTurns = _db.Turns.AsNoTracking().Where(t => t.UserId == residentId);
            var myPermits = _db.EntryPermissions.AsNoTracking().Where(p => p.UserId == residentId);
            var myRequests = _db.Requests.AsNoTracking().Where(r => r.UserId == residentId);

            // Última actividad: la fecha más reciente entre reservas, permisos y solicitudes creadas
            var lastTurn = await myTurns.MaxAsync(t => (DateTime?)t.CreatedAt);
            var lastPermit = await myPermits.MaxAsync(p => (DateTime?)p.CreatedAt);
            var lastRequest = await myRequests.MaxAsync(r => (DateTime?)r.CreatedAt);
            var lastActivity = new[] { lastTurn, lastPermit, lastRequest }.Max();

            return new ResidentStatsDto
            {
                UpcomingReservations = await myTurns.CountAsync(t => t.TurnStatus != TurnStatus.Cancelled && t.StartTime >= now),
                TotalReservations = await myTurns.CountAsync(),
                ActivePermits = await myPermits.CountAsync(p => p.EntryPermissionState == PermissionStatus.Pending && (p.ValidTo == null || p.ValidTo >= now)),
                TotalPermits = await myPermits.CountAsync(),
                PendingRequests = await myRequests.CountAsync(r => r.RequestStatus == RequestStatus.Pending),
                AvailableAmenities = await _db.Amenities.AsNoTracking().CountAsync(),
                Properties = await _db.Properties.AsNoTracking().CountAsync(p => p.UserId == residentId),
                LastActivityAt = lastActivity,
            };
        }

        public async Task<SecurityStatsDto> GetSecurityStatsAsync()
        {
            var now = DateTime.UtcNow;
            var (todayStart, todayEnd) = TodayRangeUtc();
            var permits = _db.EntryPermissions.AsNoTracking().Where(p => p.DeletedAt == null);

            return new SecurityStatsDto
            {
                PermitsToday = await permits.CountAsync(p => p.ValidFrom < todayEnd && (p.ValidTo == null || p.ValidTo >= todayStart)),
                PendingPermits = await permits.CountAsync(p => p.EntryPermissionState == PermissionStatus.Pending && (p.ValidTo == null || p.ValidTo >= now)),
                VisitorsInside = await permits.CountAsync(p => p.EntryTime != null && p.DepartureTime == null),
                CheckInsToday = await permits.CountAsync(p => p.EntryTime >= todayStart && p.EntryTime < todayEnd),
                CheckOutsToday = await permits.CountAsync(p => p.DepartureTime >= todayStart && p.DepartureTime < todayEnd),
            };
        }
    }
}
