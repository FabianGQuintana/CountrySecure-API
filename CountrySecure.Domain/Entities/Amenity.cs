using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Domain.Entities
{
    public class Amenity : BaseEntity
    {
        public required string AmenityName { get; set; }
        public required string Description { get; set; }
        public required string Schedules { get; set; }
        public required int Capacity { get; set; }
       
        public ICollection<Turn> Turns { get; set; } = new List<Turn>();


    }
}