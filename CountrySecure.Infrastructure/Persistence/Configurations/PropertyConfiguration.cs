using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;


namespace CountrySecure.Infrastructure.Persistence.Configuration;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
       public void Configure(EntityTypeBuilder<Property> builder)
       {
              // === 1. CONFIGURACIÓN DE COLUMNAS (RESTRICCIONES) ===

              builder.Property(p => p.Street)
                     .IsRequired()
                     .HasMaxLength(150);

              builder.Property(p => p.PropertyNumber)
                     .IsRequired();

              builder.Property(p => p.Status)
                  .HasConversion<string>() // <- Instruye a EF Core a guardar el Enum como string ("Occupied", "NewBrand")
                  .IsRequired();

              // === 2. UNICIDAD (RESTRICCIÓN DE DOMINIO) ===
              // La combinación de calle y número de casa debe ser única.
              builder.HasIndex(p => new { p.Street, p.PropertyNumber })
                     .IsUnique();

              // === 3. RELACIONES 1:N ===

              // Relación User a Property
              builder.HasOne(p => p.User)
                     .WithMany(u => u.Properties)
                     .HasForeignKey(p => p.UserId)
                     .OnDelete(DeleteBehavior.Restrict); // Previene que al borrar un User se borren sus Properties

              // Relación Lot a Property
              builder.HasOne(p => p.Lot)
                     .WithMany(l => l.Properties)
                     .HasForeignKey(p => p.LotId)
                     .OnDelete(DeleteBehavior.Restrict);
       }
}
