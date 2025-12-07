using System;
using System.Collections.Generic;

namespace CountrySecure.Application.DTOs.Amenity
{
    public class AmenityResponseDto
    {
        public Guid Id { get; set; }
        public required string AmenityName { get; set; }
        public required string Description { get; set; }
        public required string Schedules { get; set; }
        public required int Capacity { get; set; }
        public required string Status { get; set; } // De BaseEntity, útil para el cliente

        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

    }
}
