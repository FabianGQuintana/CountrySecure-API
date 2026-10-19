namespace CountrySecure.Domain.Entities
{
    /// <summary>
    /// Base de todas las entidades. La baja lógica se expresa ÚNICAMENTE con <see cref="DeletedAt"/>:
    /// null = activa, con fecha = dada de baja. EF aplica un filtro global que oculta las dadas de baja.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "system";
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }

        // Baja lógica
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        /// <summary>Da de baja la entidad (idempotente).</summary>
        public void SoftDelete(string? modifiedBy = null)
        {
            var now = DateTime.UtcNow;
            DeletedAt ??= now;
            Touch(modifiedBy, now);
        }

        /// <summary>Reactiva una entidad dada de baja.</summary>
        public void Restore(string? modifiedBy = null)
        {
            DeletedAt = null;
            Touch(modifiedBy, DateTime.UtcNow);
        }

        public void Touch(string? modifiedBy, DateTime? at = null)
        {
            var now = at ?? DateTime.UtcNow;
            UpdatedAt = now;
            LastModifiedAt = now;
            if (modifiedBy != null) LastModifiedBy = modifiedBy;
        }
    }
}
