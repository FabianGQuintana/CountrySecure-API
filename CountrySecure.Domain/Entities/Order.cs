using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    public class Order: BaseEntity
    {
        //Attributes
        public required string Description { get; set; }
        public required string SupplierName { get; set; }
        public OrderStatus OrderType { get; set; }

        // Relashionships
        public ICollection<Request>? Requests { get; set; }
        public ICollection<EntryPermission>? EntryPermissions { get; set; }
    }
}
