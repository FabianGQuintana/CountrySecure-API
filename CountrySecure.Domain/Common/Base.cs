namespace CountrySecure.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Si IsDeleted tiene un valor (significa que fue dado de baja), entonces IsDeleted es true
        public bool IsDeleted => DeletedAt.HasValue;
    }
}