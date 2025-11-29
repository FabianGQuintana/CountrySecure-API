using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Properties
{
    public class CreatePropertyDto
    {
        [Required]
        [StringLength(150)]
        public string Street { get; set; }

        [Required]
        public int HouseNumber { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        public int IdLot { get; set; } 
    }
}