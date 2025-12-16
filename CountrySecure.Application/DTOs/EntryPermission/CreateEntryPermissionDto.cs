using CountrySecure.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class CreateEntryPermissionDto
    {

        [Required]
        public  PermissionType PermissionType { get; set; }

        [StringLength(600)]
        public string? Description { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }
        public DateTime? EntryTime { get; set; } = null;
        public DateTime? DepartureTime { get; set; } = null;

        public PermissionStatus Status { get; set; } = PermissionStatus.Pending;

        
        [Required]
        public required Guid VisitId { get; set; }

        [Required]
        public required Guid UserId { get; set; }

        public Guid? OrderId { get; set; }

    }
}
