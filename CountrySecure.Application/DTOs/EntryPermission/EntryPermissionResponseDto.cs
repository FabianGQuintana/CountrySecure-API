using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class EntryPermissionResponseDto
    {
        public Guid Id { get; set; }

        public required string QrCodeValue { get; set; }

        public PermissionType Type { get; set; }
        public PermissionStatus Status { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime DepartureTime { get; set; }

        public required string BaseEntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }

        // 3. Propiedades de Navegación (DTOs Anidados)

        // El Residente que generó el permiso (FK: id_usuario)
        public required EntryPermissionUserDto Resident { get; set; }

        // El Visitante al que se le dio el permiso (FK: id_visita)
        public required EntryPermissionVisitDto Visitor { get; set; }

        // Información del Servicio si aplica (FK: id_servicio, puede ser null)
        public EntryPermissionOrderDto? Order { get; set; }
    }
}
