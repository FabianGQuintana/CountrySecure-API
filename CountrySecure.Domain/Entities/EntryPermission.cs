using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    public class EntryPermission : BaseEntity
    {
        public required string QrCodeValue { get; set; }

        public PermissionType PermissionType { get; set; }
        public PermissionStatus Status { get; set; }
        public string? Description { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime DepartureTime { get; set; }

        //Relationships FK
        public Guid VisitId { get; set; }

        public Guid UserId { get; set; }

        //Puede ser opcional esta FK
        public Guid? OrderId { get; set; }

        //Navigation Properties
        public  Visit? Visit { get; set; }
        public  User? User { get; set; }

        public  Order? Order { get; set; }

    }
}
