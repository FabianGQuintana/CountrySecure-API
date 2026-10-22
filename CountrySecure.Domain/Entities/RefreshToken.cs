namespace CountrySecure.Domain.Entities
{
    public class RefreshToken: BaseEntity
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }

        // Rotación: cuándo se consumió y por cuál token fue reemplazado.
        // Permite una ventana de gracia para refrescos simultáneos (varias requests con el mismo token).
        public DateTime? UsedAt { get; set; }
        public string? ReplacedByToken { get; set; }


        // FK
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}