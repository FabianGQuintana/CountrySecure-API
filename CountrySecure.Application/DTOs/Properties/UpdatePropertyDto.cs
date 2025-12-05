using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Properties
{
    public class UpdatePropertyDto
    {
        public  string? Street { get; set; }

        public int? NumberProperty { get; set; }

        public PropertyStatus? Status { get; set; }

        // FKs  
        public Guid? UserId { get; set; }
        public Guid? LotId { get; set; }


    }
}
