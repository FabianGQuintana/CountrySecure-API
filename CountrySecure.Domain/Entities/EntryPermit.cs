using System;
using CountrySecure.Domain.Enums;
namespace CountrySecure.Domain.Entities;
public class EntryPermit : BaseEntity
{
    // === Campos de la tabla ===

    public string QR { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? HorarioIngreso { get; set; }
    public DateTime? HorarioSalida { get; set; }

    public DateTime FechaVisita { get; set; }
    public EntryPermitState Status { get; set; } = EntryPermitState.Pending;

    //relationships

    // Visitante (IdVisit en tu modelo)
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; }

    // Usuario que creó o gestiona el permiso
    public Guid UserId { get; set; }
    public User User { get; set; }

    // Servicio (opcional)
    public Guid? ServiceId { get; set; }
//    public Service Service { get; set; }

    // Si existe una referencia a un permiso padre (auto-relación):
    public Guid? ParentPermitId { get; set; }
    public EntryPermit ParentPermit { get; set; }
}
