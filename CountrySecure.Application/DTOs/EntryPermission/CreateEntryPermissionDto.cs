using CountrySecure.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class CreateEntryPermissionDto
    {
        [Required]
        public required string QrCodeValue { get; set; }

        [Required]
        public  PermissionType PermissionType { get; set; }

        [StringLength(600)]
        public string? Description { get; set; }

        [Required]
        public required Guid VisitId { get; set; }

        [Required]
        public required Guid UserId { get; set; }

        public Guid? ServiceId { get; set; }

    }
}
