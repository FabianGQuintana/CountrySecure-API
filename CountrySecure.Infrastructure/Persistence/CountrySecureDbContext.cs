using System.Linq.Expressions;
using System.Reflection;
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

        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<EntryPermission> EntryPermissions { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);

            ApplySoftDeleteFilter(modelBuilder);
        }

        /// <summary>
        /// Baja lógica automática: toda entidad que herede de <see cref="BaseEntity"/> oculta las filas con
        /// DeletedAt != null en cualquier consulta. Para verlas hay que usar IgnoreQueryFilters().
        /// </summary>
        private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) || entityType.BaseType != null)
                    continue;

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var deletedAt = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var isActive = Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTime?)));

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(isActive, parameter));
            }
        }
    }
}
