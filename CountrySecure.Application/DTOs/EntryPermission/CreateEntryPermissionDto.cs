using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class CreateEntryPermissionDto
    {
        public PermissionType PermissionType { get; set; }

        public string? Description { get; set; }

        /// <summary>Cantidad de acompañantes (sin contar a la visita).</summary>
        public int CompanionsCount { get; set; }

        public VehicleType VehicleType { get; set; } = VehicleType.OnFoot;

        /// <summary>Patente/matrícula. Obligatoria si la visita llega en auto o moto.</summary>
        public string? VehiclePlate { get; set; }
        public string? VehicleModel { get; set; }

        public PassType PassType { get; set; } = PassType.SingleEntry;

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
