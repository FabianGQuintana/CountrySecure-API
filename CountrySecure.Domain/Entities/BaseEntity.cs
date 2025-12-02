using System;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Interfaces;

namespace CountrySecure.Domain.Entities
{
        // Clase Base para todas las entidades
        public abstract class BaseEntity : IStatusEntity
    {
            // 1. Clave Primaria (Identificador Único)
            public Guid Id { get; set; } = new Guid();

            public  string Status { get; set; } = "Active";

            public DateTime? UpdatedAt { get; set; }
            public DateTime? DeletedAt { get; set; }
            

            public bool IsDeleted => DeletedAt.HasValue;

        // 2. Auditoría (Fecha de Creación/Modificación)
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public  string CreatedBy { get; set; }

            public DateTime? LastModifiedAt { get; set; } = DateTime.UtcNow;
            public string? LastModifiedBy { get; set; }
        }
    }
