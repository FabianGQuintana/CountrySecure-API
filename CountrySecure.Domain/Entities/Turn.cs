using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    public class Turn : BaseEntity
    {
        // --- Nuevas Propiedades de Negocio ---
        public DateTime StartTime { get; set; } // Fecha y hora de inicio del turno
        public DateTime EndTime { get; set; }   // Fecha y hora de fin del turno (útil para calcular la duración)

        public TurnStatus Status { get; set; } = TurnStatus.Pending;
        // FKs
        public Guid AmenityId { get; set; }
        public Guid UserId { get; set; }

        // Navigation Properties
        public Amenity? Amenity { get; set; }
        public User? User { get; set; }
    }
}