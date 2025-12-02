using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Entities
{
    public class Lot : BaseEntity
    {
        public required string LotName { get; set; }

        public required string BlockName { get; set; }

        public required ICollection<Property> Properties { get; set; } = new List<Property>();

    }
}
