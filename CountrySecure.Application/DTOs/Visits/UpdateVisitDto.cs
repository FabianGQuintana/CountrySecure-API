using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Visits
{
    public class UpdateVisitDto
    {
        public string NameVisit { get; set; }
        public string LastNameVisit { get; set; }
        public int DniVisit { get; set; }
    }
}
