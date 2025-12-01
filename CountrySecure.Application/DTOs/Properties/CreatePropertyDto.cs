using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Properties
{
    public class CreatePropertyDto
    {
        [Required]
        [StringLength(150)]
        public required string Street { get; set; }

        [Required]
        public int HouseNumber { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid LotId { get; set; }
    }
}