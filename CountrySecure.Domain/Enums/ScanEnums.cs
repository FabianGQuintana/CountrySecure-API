namespace CountrySecure.Domain.Enums
{
    /// <summary>Qué quiere registrar el guardia al escanear un pase.</summary>
    public enum ScanAction
    {
        CheckIn = 1,
        CheckOut = 2
    }

    /// <summary>Resultado de un escaneo.</summary>
    public enum ScanOutcome
    {
        CheckIn = 1,
        CheckOut = 2,
        Rejected = 3
    }

    /// <summary>Motivo del rechazo (None cuando el escaneo fue aprobado).</summary>
    public enum ScanReason
    {
        None = 0,
        InvalidToken = 1,
        NotFound = 2,
        Cancelled = 3,
        AlreadyUsed = 4,
        AlreadyInside = 5,
        NotInside = 6,
        NotYetValid = 7,
        Expired = 8,
        Blocked = 9
    }
}
