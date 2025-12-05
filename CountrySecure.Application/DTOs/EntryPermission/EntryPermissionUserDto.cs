using System;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class EntryPermissionUserDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Lastname { get; set; }
        public required int Dni { get; set; }
        public required string Phone { get; set; }
        public required string Email { get; set; }
    }
}