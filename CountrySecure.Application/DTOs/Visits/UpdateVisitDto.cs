using System;

namespace CountrySecure.Application.DTOs.Visits
{
    public class UpdateVisitDto
    {

        public string? NameVisit { get; set; }
        public string? LastNameVisit { get; set; }
        public int? DniVisit { get; set; }
        public string? StatusVisit { get; set; }
    }
}
