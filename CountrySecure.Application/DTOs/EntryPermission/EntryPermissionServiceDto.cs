using System;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class EntryPermissionServiceDto
    {
        public Guid Id { get; set; }
        public required string BusinessName { get; set; } // Nombre del rubro
        public string? Description { get; set; }
        public required string Status { get; set; }
    }
}