using System;

namespace CountrySecure.Application.DTOs.Visits
{
    public class CreateVisitDto
    {
        public required string NameVisit { get; set; }
        public required string LastNameVisit { get; set; }
        public int DniVisit { get; set; }
    }
}

