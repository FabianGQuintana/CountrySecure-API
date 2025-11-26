using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Lastname { get; set; }

        public int? Dni { get; set; }

        public string? Phone { get; set; }
        public string? Email { get; set; }

        public RoleEnum? Role { get; set; }
    }
}