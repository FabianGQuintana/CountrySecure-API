using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CountrySecure.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CountrySecureDbContext>
    {
        public CountrySecureDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CountrySecureDbContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5433;Database=CountrySecureDB;Username=postgres;Password=postgres");

            return new CountrySecureDbContext(optionsBuilder.Options);
        }
    }
}