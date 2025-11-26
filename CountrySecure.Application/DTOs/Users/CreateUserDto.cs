using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Users
{
    public class CreateUserDto
    {
        public required string Name { get; set; }
        public required string Lastname { get; set; }

        public required int Dni { get; set; }

        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public RoleEnum Role { get; set; }
    }
}