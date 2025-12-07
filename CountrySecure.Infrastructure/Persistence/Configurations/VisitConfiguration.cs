using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace CountrySecure.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {

        builder.HasKey(v => v.Id);
        // === 1. CONFIGURACIÓN DE COLUMNAS (RESTRICCIONES) ===
        builder.Property(v => v.NameVisit)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(v => v.LastNameVisit)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(v => v.DniVisit)
               .IsRequired();
        // === 2. UNICIDAD (RESTRICCIÓN DE DOMINIO) ===
        // El DNI del visitante debe ser único.
        builder.HasIndex(v => v.DniVisit)
               .IsUnique();
        // === 3. RELACIONES 1:N ===
        // Relación Visit a EntryPermission
         builder.HasMany(v => v.EntryPermissions)
              .WithOne(ep => ep.Visit)
              .HasForeignKey(ep => ep.VisitId)
               .OnDelete(DeleteBehavior.Cascade); // Al borrar un Visit, se borran sus EntryPermits
    }
}
