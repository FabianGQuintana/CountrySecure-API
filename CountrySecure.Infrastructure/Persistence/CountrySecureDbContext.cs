using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Infrastructure.Persistence
{
    public class CountrySecureDbContext : DbContext
    {
        public CountrySecureDbContext(DbContextOptions<CountrySecureDbContext> options)
            : base(options)
        {
        }

        // Acá van todos los DBSet
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicará TODAS las configuraciones (cuando empieces a crearlas)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CountrySecureDbContext).Assembly);
        }
    }
}