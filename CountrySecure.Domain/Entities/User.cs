
using CountrySecure.Domain.Constants;


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
        public bool Active { get; set; } = true;


        public required string Role { get; set; }

        // Opcional: info del admin que crea o modifica
        // public Guid? CreatedBy { get; set; }
        // public Guid? UpdatedBy { get; set; }

    }
}
