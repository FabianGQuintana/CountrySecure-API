using System;

namespace CountrySecure.Application.DTOs.EntryPermission
{
    public class EntryPermissionVisitDto
    {
        public Guid Id { get; set; }
        public required string NameVisit { get; set; }
        public required string LastNameVisit { get; set; }
        public required int DniVisit { get; set; }
    }
}