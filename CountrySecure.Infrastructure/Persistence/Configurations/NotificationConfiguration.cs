using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).HasMaxLength(120).IsRequired();
            builder.Property(n => n.Message).HasMaxLength(400).IsRequired();
            builder.Property(n => n.Type).HasMaxLength(30).IsRequired();

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(n => new { n.UserId, n.ReadAt });
            builder.HasIndex(n => n.CreatedAt);
        }
    }
}
