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

        // Obsoleto: se ignora. El guardia se obtiene del token JWT (ICurrentUserService).
        // Se mantiene opcional para no romper clientes que todavía lo envían.
        public Guid? GuardId { get; set; }
    }

}
