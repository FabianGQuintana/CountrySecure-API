using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
       public class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
       {
              public void Configure(EntityTypeBuilder<Amenity> builder)
              {
                     // Configuración de la clave primaria
                     builder.HasKey(a => a.Id);

                     // Configuración de propiedades
                     builder.Property(a => a.AmenityName)
                            .IsRequired()
                            .HasMaxLength(100);

                     builder.Property(a => a.Description)
                            .IsRequired()
                            .HasMaxLength(500);

                     builder.Property(a => a.Schedules)
                            .IsRequired()
                            .HasMaxLength(200);

                     builder.Property(a => a.Capacity)
                            .IsRequired();

                     // Configuración de la relación con Turns
                     builder.HasMany(a => a.Turns)
                            .WithOne(t => t.Amenity)
                            .HasForeignKey(t => t.AmenityId)
                            .OnDelete(DeleteBehavior.Restrict);
              }

       }
}