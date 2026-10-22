namespace CountrySecure.Domain.Entities
{
    /// <summary>
    /// Enlace de recuperación de contraseña de un solo uso. Se guarda solo el hash SHA-256
    /// del token: quien lea la base no puede usar los enlaces pendientes.
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        public required string TokenHash { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? UsedAt { get; set; }

        // FK
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
