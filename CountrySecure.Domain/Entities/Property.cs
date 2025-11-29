using System;
using CountrySecure.Domain.Enums;
namespace CountrySecure.Domain.Entities;

public class Property : BaseEntity
{
	public int IdProperty { get; set; }
	public  string Street { get; set; }
	public int NumberProperty { get; set; }

    //Relationships FK
    public int IdUser { get; set; }
	public int IdLot { get; set; }

    //Navigation Properties
    public User User { get; set; }
	public Lot Lot { get; set; }
}
