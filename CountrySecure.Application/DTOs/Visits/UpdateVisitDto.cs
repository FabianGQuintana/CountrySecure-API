using System;

namespace CountrySecure.Application.DTOs.Visits
{
    public class UpdateVisitDto
    {
        public Guid VisitId { get; set; }
        public string? NameVisit { get; set; }
        public string? LastNameVisit { get; set; }
        public int? DniVisit { get; set; }
    }
}
