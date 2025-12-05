using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;

public class Visit : BaseEntity
{       
    public required string NameVisit { get; set; }
    public required string LastNameVisit { get; set; }
    public required int DniVisit { get; set; }

    //relationships
<<<<<<< Updated upstream
    // public ICollection<EntryPermit> EntryPermits { get; set; }
=======
    public ICollection<EntryPermission>? EntryPermissions { get; set; }
>>>>>>> Stashed changes
}

