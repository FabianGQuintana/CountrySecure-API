namespace CountrySecure.Domain.Entities
{
    public class RefreshToken: BaseEntity
    {
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }


        // FK
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}