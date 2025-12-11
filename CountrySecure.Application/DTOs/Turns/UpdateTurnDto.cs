// UpdateTurnDto.cs (Modificado)
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.DTOs.Turns
{
    public class UpdateTurnDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public TurnStatus? TurnStatus { get; set; }

        public Guid? AmenityId { get; set; }
        public Guid? UserId { get; set; }
    }

}


