using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;

public class Visit : BaseEntity
{       
    public string NameVisit { get; set; }
    public string LastNameVisit { get; set; }
    public int DniVisit { get; set; }

    //relationships
    public ICollection<EntryPermit> EntryPermits { get; set; }
}

