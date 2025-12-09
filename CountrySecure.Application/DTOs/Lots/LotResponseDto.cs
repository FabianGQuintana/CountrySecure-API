using CountrySecure.Domain.Enums;


namespace CountrySecure.Application.DTOs.Lots
{
    public class LotResponseDto
    {
        public Guid LotId { get; set; }

        public required string LotName { get; set; }

        public required string BlockName { get; set; }

        public required string Status { get; set; }

        public LotStatus LotState { get; set; }
        public DateTime CreatedAt { get; internal set; }
    }
}
