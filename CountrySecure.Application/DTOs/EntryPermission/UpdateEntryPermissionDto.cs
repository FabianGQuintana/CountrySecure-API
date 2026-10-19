using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    /// <summary>
    /// Edición de un permiso por parte de su dueño (o un Admin). Los ingresos y egresos NO se editan acá:
    /// se registran con los endpoints de check-in / check-out de portería.
    /// </summary>
    public class UpdateEntryPermissionDto
    {
        public PermissionType? PermissionType { get; set; }
        public string? Description { get; set; }
        public int? CompanionsCount { get; set; }
        public VehicleType? VehicleType { get; set; }
        public string? VehiclePlate { get; set; }
        public string? VehicleModel { get; set; }
        public PassType? PassType { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public Guid? OrderId { get; set; }
    }
}
