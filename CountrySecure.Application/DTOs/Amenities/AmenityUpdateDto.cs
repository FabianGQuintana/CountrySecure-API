using System;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Amenity
{
    public class AmenityUpdateDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string AmenityName { get; set; }

        [StringLength(500)]
        public required string Description { get; set; }

        [StringLength(200)]
        public required string Schedules { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }

        // El cliente también puede querer cambiar el status o activo
        public required string Status { get; set; }
    }
}