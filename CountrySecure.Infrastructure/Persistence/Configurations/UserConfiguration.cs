

using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CountrySecure.Infrastructure.Persistence.Configuration
{
       public class UserConfiguration : IEntityTypeConfiguration<User>
       {
              public void Configure(EntityTypeBuilder<User> builder)
              {

                     // nombre de la tabla
                     builder.ToTable("Users");

                     // PK de BaseEntity (Guid)
                     builder.HasKey(u => u.Id);

                     // name
                     builder.Property(u => u.Name)
                            .IsRequired()
                            .HasMaxLength(100);

                     // lastname
                     builder.Property(u => u.Lastname)
                            .IsRequired()
                            .HasMaxLength(100);

                     // DNI
                     builder.Property(u => u.Dni)
                            .IsRequired();
                     builder.HasIndex(u => u.Dni)    // El dni es unico
                            .IsUnique();

                     // Phone
                     builder.Property(u => u.Phone)
                            .IsRequired()
                            .HasMaxLength(30);

                     // EMAIL
                     builder.Property(u => u.Email)
                            .IsRequired()
                            .HasMaxLength(150);

                     builder.HasIndex(u => u.Email)
                            .IsUnique();

                     // PASSWORD
                     builder.Property(u => u.Password)
                            .IsRequired()
                            .HasColumnType("text");

                     // ACTIVE
                     builder.Property(u => u.Active)
                            .IsRequired();

                     // RELACIÓN: User 1 -- * Property
                     builder.HasMany(u => u.Properties)
                            .WithOne(p => p.User)
                            .HasForeignKey(p => p.UserId)
                            .OnDelete(DeleteBehavior.Restrict);
                     // Restrict evita que eliminar un usuario borre todas las propiedades

                     builder.HasMany(u => u.EntryPermissions)
                            .WithOne(ep => ep.User)
                            .HasForeignKey(ep => ep.UserId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasMany(u => u.Turns)
                            .WithOne(t => t.User)
                            .HasForeignKey(t => t.UserId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasMany(u => u.RefreshTokens)
                            .WithOne(rt => rt.User)
                            .HasForeignKey(rt => rt.UserId)
                            .OnDelete(DeleteBehavior.Cascade);
              }
       }
}