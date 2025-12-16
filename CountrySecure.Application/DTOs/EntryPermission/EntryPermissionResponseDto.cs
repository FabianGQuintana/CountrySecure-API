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
        public DateTime? EntryTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ValidTo { get; set; } 
        public required string BaseEntityStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedBy { get; set; }

        // 3. Propiedades de Navegación (DTOs Anidados)
        public required EntryPermissionUserDto Resident { get; set; }
        public required EntryPermissionVisitDto Visitor { get; set; }
        public EntryPermissionOrderDto? Order { get; set; }

        // Si la entidad EntryPermission tiene las propiedades de navegación CheckInGuard / CheckOutGuard
        // debemos añadir DTOs para mapear sus nombres.

        // Propiedad 1: Guardia que realizó la Entrada
        public EntryPermissionGuardDto? CheckInGuard { get; set; }

        // Propiedad 2: Guardia que realizó la Salida
        public EntryPermissionGuardDto? CheckOutGuard { get; set; }

    

        // DTO AUXILIAR (Necesitas crear esta clase o usar EntryPermissionUserDto)

        public class EntryPermissionGuardDto
        {
            public Guid Id { get; set; }
            public required string Name { get; set; }
            public required string Lastname { get; set; }
            public required int dni { get; set; }

        }


    }
}
