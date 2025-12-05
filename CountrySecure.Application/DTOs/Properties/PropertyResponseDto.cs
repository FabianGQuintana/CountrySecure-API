using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Properties
{
    public class PropertyResponseDto
    {
        public Guid PropertyId { get; set; }
        public required string Street { get; set; }
        public int HouseNumber { get; set; }
        public PropertyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}