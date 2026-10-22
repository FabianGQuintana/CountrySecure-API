namespace CountrySecure.Domain.Entities
{
    public class User : BaseEntity
    {
        public required string Name { get; set; }
        public required string Lastname { get; set; }

        public required int Dni { get; set; }

        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }


        public required string Role { get; set; }

        /// <summary>Un residente bloqueado no puede recibir visitas: sus pases se rechazan en la guardia.</summary>
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }


        // FK
        public ICollection<Property> Properties { get; set; } = new List<Property>();
        public ICollection<EntryPermission> EntryPermissions { get; set; } = new List<EntryPermission>();
        public ICollection<Turn> Turns {get; set;} = new List<Turn>();
        public ICollection<RefreshToken> RefreshTokens { get; set;} = new List<RefreshToken>();
    }
}
