using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class CreateEntryPermissionDto
    {
        public PermissionType PermissionType { get; set; }

        public string? Description { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public required Guid VisitId { get; set; }

        /// <summary>
        /// Residente titular del permiso. Un residente siempre crea permisos a su nombre
        /// (se ignora este valor); solo un Admin puede indicar otro residente.
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>Servicio asociado; obligatorio para permisos de mantenimiento.</summary>
        public Guid? OrderId { get; set; }
    }
}
