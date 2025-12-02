using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.DTOs.Lots
{
    public class LotResponseDto
    {
        public Guid LotId { get; set; }

        public required string LotName { get; set; }

        public required string BlockName { get; set; }

        public LotStatus Status { get; set; }
    }
}
