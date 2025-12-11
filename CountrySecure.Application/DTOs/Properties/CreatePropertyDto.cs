using System.ComponentModel.DataAnnotations;

namespace CountrySecure.Application.DTOs.Properties
{
    public class CreatePropertyDto
    {
        public required string Street { get; set; }
        public required int PropertyNumber { get; set; }
        public required Guid LotId { get; set; }
    }

}