using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CountrySecure.Infrastructure.Persistence
{
    /// <summary>
    /// Usado solo por las herramientas de EF (dotnet ef migrations / database update).
    /// La cadena de conexión se lee de la variable de entorno ConnectionStrings__DefaultConnection;
    /// nunca se hardcodea en el código.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CountrySecureDbContext>
    {
        public CountrySecureDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Definí la variable de entorno ConnectionStrings__DefaultConnection antes de ejecutar 'dotnet ef'.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<CountrySecureDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CountrySecureDbContext(optionsBuilder.Options);
        }
    }
}
