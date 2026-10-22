namespace CountrySecure.Application.DTOs.Stats
{
    /// <summary>KPIs del panel de administración.</summary>
    public class AdminStatsDto
    {
        public int TotalResidents { get; set; }
        public int TotalSecurityStaff { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalLots { get; set; }
        public int TotalProperties { get; set; }
        public int OccupiedProperties { get; set; }
        public int TotalAmenities { get; set; }
        public int UpcomingReservations { get; set; }
        public int ReservationsToday { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveOrders { get; set; }
        public int PermitsToday { get; set; }
        public int VisitorsInside { get; set; }
    }

    /// <summary>KPIs del panel del residente autenticado.</summary>
    public class ResidentStatsDto
    {
        public int UpcomingReservations { get; set; }
        public int TotalReservations { get; set; }
        public int ActivePermits { get; set; }
        public int TotalPermits { get; set; }
        public int PendingRequests { get; set; }
        public int AvailableAmenities { get; set; }
        public int Properties { get; set; }
        public DateTime? LastActivityAt { get; set; }
    }

    /// <summary>KPIs del panel de seguridad (portería).</summary>
    public class SecurityStatsDto
    {
        public int PermitsToday { get; set; }
        public int PendingPermits { get; set; }
        public int VisitorsInside { get; set; }
        public int CheckInsToday { get; set; }
        public int CheckOutsToday { get; set; }
    }
}
