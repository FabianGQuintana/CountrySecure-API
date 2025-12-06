using System;
using CountrySecure.Domain.Enums;
namespace CountrySecure.Domain.Entities;

public class Property : BaseEntity
{
	public required string Street { get; set; }
	public int PropertyNumber { get; set; }

    public PropertyStatus PropertyType { get; set; }

    //Relationships FK
    public Guid UserId { get; set; }
	public Guid LotId { get; set; }

    //Navigation Properties
    public  User? User { get; set; }
	public  Lot? Lot { get; set; }
}
