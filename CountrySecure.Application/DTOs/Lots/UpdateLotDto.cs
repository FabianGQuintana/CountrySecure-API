
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Lots
{
    public class UpdateLotDto
    {
        public  string? LotName { get; set; }

        public  string? BlockName { get; set; }

        public LotStatus? Status { get; set; }


    }
}
