using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Amenity
{
    public class AmenityCreateDto
    {
        [Required]
        [StringLength(100)]
        public  required string AmenityName { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        [Required]
        [StringLength(200)]
        public required string Schedules { get; set; }

        [Required]
        [Range(1, 100)] 
        public int Capacity { get; set; }

        [Required]
        public ICollection<TurnDto> Turns { get; set; } = new List<TurnDto>();
    }
}
