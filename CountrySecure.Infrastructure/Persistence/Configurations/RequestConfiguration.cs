using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            // Configuración de la clave primaria
            builder.HasKey(r => r.Id);

            // Configuración de la propiedad 'Details' (campo obligatorio y con longitud máxima)
            builder.Property(r => r.Details)
                   .IsRequired()  
                   .HasMaxLength(500);  

            // Configuración de la propiedad 'Location' 
            builder.Property(r => r.Location)
                   .IsRequired()
                   .HasMaxLength(300);  

            // Configuración de la propiedad 'EntryPermissionState'
            builder.Property(r => r.Status)
                   .HasConversion<string>()  
                   .IsRequired();

            // Relación uno a muchos con User (Una solicitud tiene un único usuario)
            builder.HasOne(r => r.User)
                   .WithMany()  // No es necesario especificar una colección en User
                   .HasForeignKey(r => r.IdUser)
                   .OnDelete(DeleteBehavior.Cascade);  // Si el usuario se elimina, se eliminan las solicitudes asociadas

            // Relación uno a muchos con Order (Una solicitud está asociada a una única orden)
            builder.HasOne(r => r.Order)
                   .WithMany(o => o.Requests)  // Una orden tiene muchas solicitudes
                   .HasForeignKey(r => r.IdOrder)
                   .OnDelete(DeleteBehavior.Restrict);  // No se puede eliminar una orden si tiene solicitudes asociadas
        }
    }
}
