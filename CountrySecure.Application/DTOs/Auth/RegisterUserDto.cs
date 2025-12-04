
namespace CountrySecure.Application.DTOs.Auth
{
    public class RegisterUserDto
    {
        public required string Name { get; set; }
        public required string Lastname { get; set; }

        public required int Dni { get; set; }

        public required string Phone { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }

        public required string Role { get; set; }
    }
}