namespace CountrySecure.Application.DTOs.Turns
{
    // DTO para la respuesta del Turno
    public class TurnResponseDto
    {
        // --- Propiedades Base (Auditoría) ---
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }

        // --- Propiedades de Negocio ---
        public DateTime StartTime { get; set; } // Fecha y hora de inicio
        public DateTime EndTime { get; set; }   // Fecha y hora de fin
        public required string Status { get; set; } // El estado del turno (ej: "Pending", "Completed")

        // --- Claves Foráneas (FKs) ---
        public Guid AmenityId { get; set; }
        public Guid UserId { get; set; }

        // --- Propiedades de Navegación (Información Relacionada) ---
        public AmenityReferenceDto? Amenity { get; set; } // Detalles esenciales de la Amenity
        public UserReferenceDto? User { get; set; }       // Detalles esenciales del Usuario
    }
}