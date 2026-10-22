using System;

namespace CountrySecure.Application.DTOs.Visits
{
    public class VisitResponseDto
    {
        public Guid VisitId { get; set; }
        public required string NameVisit { get; set; }
        public required string LastNameVisit { get; set; }
        public int DniVisit { get; set; }
        public required string VisitStatus { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }

    }
}
