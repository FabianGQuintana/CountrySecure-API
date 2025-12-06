using Microsoft.EntityFrameworkCore;
using System.Reflection;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Persistence
{
    public class CountrySecureDbContext : DbContext
    {
        public CountrySecureDbContext(DbContextOptions<CountrySecureDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users {get; set;}
        public DbSet<Property> Properties { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<EntryPermission> EntryPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
