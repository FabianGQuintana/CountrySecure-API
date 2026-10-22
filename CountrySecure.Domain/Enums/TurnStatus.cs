namespace CountrySecure.Domain.Enums
{
    // Mismo orden que el resto de estados: 0 = baja/cancelado, 1 = pendiente, 2 = completado
    public enum TurnStatus
    {
        Cancelled = 0,
        Pending = 1,
        Completed = 2
    }
}
