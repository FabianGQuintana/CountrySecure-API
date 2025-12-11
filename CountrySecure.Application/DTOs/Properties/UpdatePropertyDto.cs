using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Properties
{
    public class UpdatePropertyDto
    {
        public  string? Street { get; set; }

        public int? PropertyNumber { get; set; }

        public PropertyStatus? PropertyStatus { get; set; }
        public string? StatusAuditoria { get; set; }
        // FKs  
        public Guid? UserId { get; set; }
        public Guid? LotId { get; set; }


    }
}
