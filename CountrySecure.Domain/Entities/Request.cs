using CountrySecure.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Entities
{
    public class Request : BaseEntity
    { 
        public required string Details { get; set; }
        public required string Location { get; set; }
        public RequestStatus RequestStatus { get; set; } = RequestStatus.Pending;

        //relationships
        public Guid IdUser { get; set; }
        public required User UserRequest { get; set; }

        public Guid IdOrder { get; set; }
        public  required Order OrderRequest { get; set; }
    }
}
