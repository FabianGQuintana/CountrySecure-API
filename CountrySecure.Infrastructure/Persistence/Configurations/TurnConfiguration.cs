using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence.Configurations
{
    public class TurnConfiguration : IEntityTypeConfiguration<Turn>
    {
        public void Configure(EntityTypeBuilder<Turn> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.StartTime).IsRequired();
            builder.Property(t => t.EndTime).IsRequired();

            builder.HasOne(t => t.Amenity)
                   .WithMany(a => a.Turns)
                   .HasForeignKey(t => t.AmenityId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.User)
                   .WithMany(u => u.Turns)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}