using System;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    // Nota: Los campos deben ser nulleables para permitir actualizaciones parciales (PATCH).
    public class UpdateEntryPermissionDto
    {
        // El ID de la Entidad que queremos actualizar es el que se pasa al método del servicio
        // (Task<T?> UpdateAsync(UpdatePropertyDto dto, Guid entryPermissionId))
        
        public string? QrCodeValue { get; set; }

        // NOTA: Si el enum no es nulleable, debe ser la versión DTO que maneje la nulidad
        public PermissionType? PermissionType { get; set; }

        public string? Description { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public PermissionStatus? Status { get; set; }


        // Claves foráneas (FKs) que se podrían reasignar
        public Guid? VisitId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? OrderId { get; set; }
    }
}