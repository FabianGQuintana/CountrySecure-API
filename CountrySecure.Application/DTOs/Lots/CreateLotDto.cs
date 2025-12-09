using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Lots
{
    public class CreateLotDto
    {
        public required string LotName { get; set; }

        public required string BlockName { get; set; }

        public LotStatus LotState { get; set; }


    }
}
