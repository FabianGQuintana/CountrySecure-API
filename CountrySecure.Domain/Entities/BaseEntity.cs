using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    public abstract class BaseEntity 
    {
        public int Id { get; set; }
        public PropertyStatus Status { get; set; }
        // Aquí también podrías poner DateTime CreatedAt, string CreatedBy, etc.
    }
}