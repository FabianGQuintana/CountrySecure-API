using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.EntryPermit
{
    public class UpdateEntryPermitDto
    {   
        public Guid EntryPermitId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }

    }
}
