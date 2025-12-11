using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Necesario para EnumToStringConverter

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class EntryPermissionConfiguration : IEntityTypeConfiguration<EntryPermission>
    {
        public void Configure(EntityTypeBuilder<EntryPermission> builder)
        {
            // 1. Clave Primaria (Heredada de BaseEntity)
            builder.HasKey(ep => ep.Id);

            // 2. Mapeo de Enums a String (para legibilidad en la DB)

            // a) PermissionType (Tipo de permiso: Visitante, Servicio, etc.)
            builder.Property(ep => ep.PermissionType)
                .HasConversion<string>()
                .IsRequired();

            // b) EntryPermissionState (Estado del permiso: Pending, Used, Cancelled)
            builder.Property(ep => ep.EntryPermissionState)
                .HasConversion<string>()
                .IsRequired();

            // 3. Configuración de Propiedades Propias

            // QrCodeValue es requerido por 'required'
            builder.Property(ep => ep.QrCodeValue)
                .HasMaxLength(100) // Valor razonable para un GUID/código QR
                .IsRequired();

            builder.Property(ep => ep.Description)
                .HasMaxLength(300); // Descripción es opcional

            // 4. Configuración de Relaciones (FKs)

            // Relación con Visit (Obligatoria: Creado para un visitante)
            builder.HasOne(ep => ep.Visit)
                .WithMany() // Assuming Visit doesn't have a collection back to EntryPermission
                .HasForeignKey(ep => ep.VisitId)
                .OnDelete(DeleteBehavior.Restrict) // No borrar Visit si tiene permisos asociados
                .IsRequired();

            // Relación con User (Obligatoria: Creado por un residente)
            builder.HasOne(ep => ep.User)
                .WithMany() // Assuming User doesn't have a collection back to EntryPermission
                .HasForeignKey(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Restrict) // No borrar User si tiene permisos asociados
                .IsRequired();

            // Relación con Order (Opcional: Asociado a un servicio)
            builder.HasOne(ep => ep.Order)
                .WithMany(o => o.EntryPermissions) // Assuming Order has a collection property called EntryPermissions
                .HasForeignKey(ep => ep.OrderId)
                .OnDelete(DeleteBehavior.SetNull); // Si la Orden es eliminada, el OrderId en el permiso se pone a NULL

            // 5. Configuración de Auditoría (BaseEntity.Status, CreatedAt, etc. no requieren configuración adicional)
        }
    }
}