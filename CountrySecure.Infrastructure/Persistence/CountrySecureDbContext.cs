using Microsoft.EntityFrameworkCore;
// using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence
{
    public class CountrySecureDbContext : DbContext
    {
        public CountrySecureDbContext(DbContextOptions<CountrySecureDbContext> options)
            : base(options)
        {
        }

        // Ejemplo de DbSet (después agregás todos)
        // public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicará TODAS las configuraciones (cuando empieces a crearlas)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CountrySecureDbContext).Assembly);
        }
    }
}