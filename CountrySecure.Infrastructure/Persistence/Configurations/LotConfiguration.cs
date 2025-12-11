using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class LotConfiguration : IEntityTypeConfiguration<Lot>
    {
        public void Configure(EntityTypeBuilder<Lot> builder)
        {
            // 1. Clave Primaria (EF Core lo infiere, pero es bueno ser explícito)
            builder.HasKey(l => l.Id);

            // 2. Unicidad
            // Es crucial que el nombre del Lote o Bloque sea único
            builder.HasIndex(l => l.LotName).IsUnique();
            builder.Property(l => l.LotName).IsRequired().HasMaxLength(100);

            // 3. Relación 1:N con Property (Navegación bidireccional)
            builder.HasMany(l => l.Properties) // Un Lote tiene muchas Propiedades
                   .WithOne(p => p.Lot)       // Una Propiedad pertenece a un Lote
                   .HasForeignKey("LotId")    // La FK está en la tabla Property (asumiendo que la columna es IdLot)
                   .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada

                    builder.Property(l => l.LotState)
                   .HasConversion<string>()
                   .IsRequired()
                   .HasMaxLength(20);
            // 4. Configuración del estado (EntryPermissionState)
            // Asumiendo que EntryPermissionState es un string en BaseEntity
            builder.Property(l => l.Status)
                   .HasMaxLength(20);
        }
    }
}