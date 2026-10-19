using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    /// <summary>
    /// Datos del pase digital: lo que se imprime en el PDF, lo que ve la visita en el enlace público
    /// y lo que el guardia ve al escanear. No incluye teléfono ni email del residente.
    /// </summary>
    public class PassDto
    {
        public Guid PermissionId { get; set; }

        /// <summary>Contenido del QR (token firmado).</summary>
        public required string Token { get; set; }

        public PermissionStatus Status { get; set; }
        public PassType PassType { get; set; }

        public required string ResidentName { get; set; }

        /// <summary>Lote / casa de destino (p. ej. "Los Aromos 123 · Lote A-12").</summary>
        public required string Destination { get; set; }

        public required string VisitorName { get; set; }
        public int VisitorDni { get; set; }
        public int CompanionsCount { get; set; }

        public VehicleType VehicleType { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleModel { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? Description { get; set; }
    }

    public class ScanRequestDto
    {
        /// <summary>Valor leído del QR.</summary>
        public required string Token { get; set; }

        /// <summary>Por defecto registra el ingreso; con CheckOut registra la salida.</summary>
        public ScanAction Action { get; set; } = ScanAction.CheckIn;

        /// <summary>true cuando el escaneo se validó sin conexión y se está sincronizando.</summary>
        public bool Offline { get; set; }

        /// <summary>Momento real del escaneo (solo se respeta en escaneos offline).</summary>
        public DateTime? ScannedAt { get; set; }
    }

    public class ScanResultDto
    {
        public bool Success { get; set; }
        public ScanOutcome Outcome { get; set; }
        public ScanReason Reason { get; set; }
        public required string Message { get; set; }
        public DateTime ScannedAt { get; set; }

        /// <summary>Ficha del pase (null si el token no corresponde a ningún permiso).</summary>
        public PassDto? Pass { get; set; }
    }

    public class AccessLogResponseDto
    {
        public Guid Id { get; set; }
        public Guid? PermissionId { get; set; }
        public Guid GuardId { get; set; }
        public required string GuardName { get; set; }
        public string? VisitorName { get; set; }
        public ScanOutcome Outcome { get; set; }
        public ScanReason Reason { get; set; }
        public string? Message { get; set; }
        public DateTime ScannedAt { get; set; }
        public bool Offline { get; set; }
    }
}
