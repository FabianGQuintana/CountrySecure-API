using System;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Amenities
{
    public class AmenityUpdateDto
    {
        // La Id se pasa al servicio a través de la URL.

        [StringLength(100)]
        public string? AmenityName { get; set; } // Hacemos nullable para PATCH

        [StringLength(500)]
        public string? Description { get; set; } // Hacemos nullable para PATCH

        [StringLength(200)]
        public string? Schedules { get; set; } // Hacemos nullable para PATCH

        [Range(1, 100)]
        public int? Capacity { get; set; } // Hacemos nullable para PATCH

        public string? Status { get; set; } // Hacemos nullable para PATCH
    }
}