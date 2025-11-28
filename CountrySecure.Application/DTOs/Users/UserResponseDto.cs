
namespace CountrySecure.Application.DTOs.Users
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public int Dni { get; set; }

        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;

        public bool Active { get; set; }

        public string Role { get; set; } = null!;

        // Auditoría básica (útil para admins)
        // public DateTime CreatedAt { get; set; }
        // public DateTime? UpdatedAt { get; set; }
    }
}
