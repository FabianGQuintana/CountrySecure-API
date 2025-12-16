using CountrySecure.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class CreateEntryPermissionDto
    {

        [Required]
        public PermissionType PermissionType { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }
        public DateTime? EntryTime { get; set; } = null;
        public DateTime? DepartureTime { get; set; } = null;

        // 🔽 DATOS DEL VISITANTE
        [Required]
        public required string NameVisit { get; set; }

        [Required]
        public required string LastNameVisit { get; set; }

        [Required]
        public required int DniVisit { get; set; }

    }
}
