using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.EntryPermit
{
    public class EntryPermitResponseDto
    {
        public Guid EntryPermitId { get; set; }
        public string QR { get; set; } = string.Empty;
        public Guid VisitId { get; set; }
        public DateTime? HorarioIngreso { get; set; }= null;
        public DateTime? HorarioSalida { get; set; }= null;
        public DateTime FechaVisita { get; set; }

        public string? Notes { get; set; }
    
        public Guid UserId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? ParentPermitId { get; set; }
        

    }


}
