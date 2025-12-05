using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    public class EntryPermission : BaseEntity
    {
        public required string QrCodeValue { get; set; }

        public PermissionType PermissionType { get; set; }

        public string? Description { get; set; }

        //Relationships FK
        public Guid VisitId { get; set; }

        public Guid UserId { get; set; }

        //Puede ser opcional esta FK
        public Guid? ServiceId { get; set; }

        //Navigation Properties
        public  Visit? Visit { get; set; }
        public  User? User { get; set; }

        public  Service? Service { get; set; }

    }
}
