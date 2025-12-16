using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class EntryPermissionConfiguration : IEntityTypeConfiguration<EntryPermission>
    {
        public void Configure(EntityTypeBuilder<EntryPermission> builder)
        {
            // 1. Clave Primaria
            builder.HasKey(ep => ep.Id);

            // 2. Mapeo de Enums a String
            builder.Property(ep => ep.PermissionType)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(ep => ep.EntryPermissionState)
                .HasConversion<string>()
                .IsRequired();

            // 3. Configuración de Propiedades Propias
            builder.Property(ep => ep.QrCodeValue)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(ep => ep.Description)
                .HasMaxLength(300);

            
            // Se puede dejar como nullable (el valor por defecto de DateTime?)
            builder.Property(ep => ep.ValidTo)
                   .IsRequired(false);


            // 4. Configuración de Relaciones (FKs)

            // Relación con Visit (Obligatoria)
            builder.HasOne(ep => ep.Visit)
                .WithMany()
                .HasForeignKey(ep => ep.VisitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relación con User (Residente - Obligatoria)
            builder.HasOne(ep => ep.User)
                .WithMany()
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relación con Order (Opcional)
            builder.HasOne(ep => ep.Order)
                .WithMany(o => o.EntryPermissions)
                .HasForeignKey(ep => ep.OrderId)
                .OnDelete(DeleteBehavior.SetNull);


            // Relación con el Guardia de Check-In (Opcional, puede ser null)
            builder.HasOne(ep => ep.CheckInGuard)
                .WithMany() // Asumimos que User no tiene una colección para CheckInGuard
                .HasForeignKey(ep => ep.CheckInGuardId)
                .OnDelete(DeleteBehavior.Restrict) // No borrar guardias si tienen logs
                .IsRequired(false); // Puede ser null

            // Relación con el Guardia de Check-Out (Opcional, puede ser null)
            builder.HasOne(ep => ep.CheckOutGuard)
                .WithMany() // Asumimos que User no tiene una colección para CheckOutGuard
                .HasForeignKey(ep => ep.CheckOutGuardId)
                .OnDelete(DeleteBehavior.Restrict) // No borrar guardias si tienen logs
                .IsRequired(false); // Puede ser null

            // 5. Configuración de Auditoría (BaseEntity.Status, CreatedAt, etc. no requieren configuración adicional)
        }
    }
}