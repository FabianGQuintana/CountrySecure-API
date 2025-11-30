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

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}