using System;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    // DTO minimalista para la verificación en la puerta
    public class GateCheckResponseDto
    {
        public Guid PermissionId { get; set; }

        // 1. Datos del Visitante (Para corroborar la identificación)
        public required string VisitorFullName { get; set; }
        public required int VisitorDni { get; set; }

        // 2. Datos del Solicitante (Para corroborar quién autorizó)
        public required string ResidentFullName { get; set; }

        // 3. Estado de Validación (La luz verde/roja)
        public required string CheckResultStatus { get; set; } // Ejemplo: "Válido y Usado", "Caducado"
        public string? Message { get; set; }
    }
}