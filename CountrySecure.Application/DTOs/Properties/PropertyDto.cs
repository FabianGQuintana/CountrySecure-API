using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Properties
{
    public class PropertyDto
    {
        public int IdProperty { get; set; }
        public string Street { get; set; }
        public int HouseNumber { get; set; }
        public PropertyStatus Status { get; set; }
    }
}