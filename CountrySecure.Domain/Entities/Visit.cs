using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using System;

namespace CountrySecure.Domain.Entities;

public class Visit : BaseEntity
{       
    public required string NameVisit { get; set; }
    public required string LastNameVisit { get; set; }
    public required int DniVisit { get; set; }

    /// <summary>Lista negra: un visitante bloqueado no puede ingresar aunque tenga un pase vigente.</summary>
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }

    //relationships
    public ICollection<EntryPermission> EntryPermissions { get; set; } = new List<EntryPermission>();
}

