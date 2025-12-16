using System;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class RegisterMovementDto
    {
        [Required]
        public Guid PermissionId { get; set; }

        [Required]
        public bool IsEntry { get; set; } // true para Entrada (Check-In), false para Salida (Check-Out)

        [Required]
        public Guid GuardId { get; set; } // ID del guardia que realiza la acción
    }

}



