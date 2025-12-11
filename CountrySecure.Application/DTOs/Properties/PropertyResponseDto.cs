using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Properties
{
    public class PropertyResponseDto
    {
        public Guid PropertyId { get; set; }
        public required string Street { get; set; }
        public int PropertyNumber { get; set; }
        public required PropertyStatus PropertyStatus { get; set; }
        public required string StatusAuditoria { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? LotName { get; set; }
        public string? OwnerName { get; set; }
    }
}