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
        public string Details { get; set; }
        public string Location { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        //relationships
        public int IdUser { get; set; }
        public User UserRequest { get; set; }

        public int IdOrder { get; set; }
        public Order OrderRequest { get; set; }
    }
}
