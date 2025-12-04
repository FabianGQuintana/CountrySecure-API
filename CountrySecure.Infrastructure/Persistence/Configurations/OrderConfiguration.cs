using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Nombre de tabla
            builder.ToTable("Orders");

            // Clave primaria heredada de BaseEntity
            builder.HasKey(o => o.Id);

            // Propiedades
            builder.Property(o => o.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.SupplierName)
                .IsRequired()
                .HasMaxLength(200);

            // Enum almacenado como string
            builder.Property(o => o.OrderType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // Auditoría (opcional dependiendo si querés configurar algo)
            builder.Property(o => o.CreatedAt).IsRequired();
            builder.Property(o => o.CreatedBy).HasMaxLength(100);
            builder.Property(o => o.LastModifiedBy).HasMaxLength(100);

            // Relaciones
            builder.HasMany(o => o.Requests)
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.EntryPermissions)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
