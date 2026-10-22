using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    /// <summary>
    /// Auditoría de portería: un registro por cada escaneo de un pase, aprobado o rechazado.
    /// Nunca se modifica ni se da de baja.
    /// </summary>
    public class AccessLog : BaseEntity
    {
        /// <summary>Permiso escaneado. Null si el token no corresponde a ningún permiso.</summary>
        public Guid? PermissionId { get; set; }

        /// <summary>Guardia que procesó el escaneo.</summary>
        public Guid GuardId { get; set; }

        public ScanOutcome Outcome { get; set; }
        public ScanReason Reason { get; set; }
        public string? Message { get; set; }

        /// <summary>Momento real del escaneo (si vino de la cola offline, el de la guardia, no el de la sincronización).</summary>
        public DateTime ScannedAt { get; set; }

        /// <summary>true si el escaneo se validó sin conexión y se sincronizó después.</summary>
        public bool Offline { get; set; }

        /// <summary>Primeros caracteres del token recibido (para rastrear intentos con tokens desconocidos).</summary>
        public string? TokenPrefix { get; set; }

        public EntryPermission? Permission { get; set; }
        public User? Guard { get; set; }
    }
}
