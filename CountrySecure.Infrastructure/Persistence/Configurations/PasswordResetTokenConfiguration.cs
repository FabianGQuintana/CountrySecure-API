using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CountrySecure.Infrastructure.Persistence.Configuration
{
    public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
            builder.HasIndex(t => t.TokenHash).IsUnique();

            builder.HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
