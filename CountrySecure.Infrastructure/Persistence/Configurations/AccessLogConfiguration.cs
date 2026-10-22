using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
    {
        public void Configure(EntityTypeBuilder<AccessLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(a => a.Reason).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(a => a.Message).HasMaxLength(300);
            builder.Property(a => a.TokenPrefix).HasMaxLength(16);

            // El historial no se pierde si el permiso o el guardia se dan de baja
            builder.HasOne(a => a.Permission)
                .WithMany()
                .HasForeignKey(a => a.PermissionId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            builder.HasOne(a => a.Guard)
                .WithMany()
                .HasForeignKey(a => a.GuardId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasIndex(a => a.ScannedAt);
            builder.HasIndex(a => a.PermissionId);
        }
    }
}
