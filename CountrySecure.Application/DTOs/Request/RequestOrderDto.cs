using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountrySecure.Application.DTOs.Request
{
    public class RequestOrderDto
    {
        public Guid Id { get; set; }
        public required string SupplierName { get; set; } 
        public required string Description { get; set; } 
        public int OrderType { get; set; }
    }
}
