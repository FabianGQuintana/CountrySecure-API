using System;
using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Amenities
{
    public class AmenityUpdateDto
    {


        [StringLength(100)]
        public string? AmenityName { get; set; } 

        [StringLength(500)]
        public string? Description { get; set; } 

        [StringLength(200)]
        public string? Schedules { get; set; } 

        [Range(1, 100)]
        public int? Capacity { get; set; } 

        public string? Status { get; set; }
    }
}