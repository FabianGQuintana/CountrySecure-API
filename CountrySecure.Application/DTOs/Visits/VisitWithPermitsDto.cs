using CountrySecure.Application.DTOs.EntryPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Visits
{
    public class VisitWithPermitsDto
    {
        public required string NameVisit { get; set; }
        public required string LastNameVisit { get; set; }
        public int DniVisit { get; set; }
        public required string VisitStatus { get; set; }

        public required List<VisitEntryPermissionDto> Permits { get; set; }
    }
}
