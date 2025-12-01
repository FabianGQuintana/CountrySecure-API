using System;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Domain.Entities
{
    // Clase Base para todas las entidades
    public abstract class BaseEntity 
    {
        // 1. Clave Primaria (Identificador Único)
        public Guid Id { get; set; } = new Guid();

        public required string Status { get; set; } = "Active";

        // 2. Auditoría (Fecha de Creación/Modificación)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; } 
    }
}