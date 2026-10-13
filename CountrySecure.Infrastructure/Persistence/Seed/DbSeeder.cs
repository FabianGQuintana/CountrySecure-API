using CountrySecure.Domain.Constants;
using CountrySecure.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CountrySecure.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// Crea el usuario Admin inicial si todavía no existe ningún Admin.
    /// Las credenciales se leen de configuración (user-secrets / variables de entorno):
    ///   Seed:AdminEmail y Seed:AdminPassword.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(CountrySecureDbContext context, IConfiguration configuration, ILogger logger)
        {
            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Seed:AdminEmail / Seed:AdminPassword no configurados. Se omite el seed del Admin.");
                return;
            }

            if (await context.Users.AnyAsync(u => u.Role == RoleTypes.Admin))
                return;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Lastname = "CountrySecure",
                Dni = 1000000,
                Phone = "0000000",
                Email = email.Trim().ToLowerInvariant(),
                Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10),
                Role = RoleTypes.Admin,
                CreatedBy = "seed"
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            logger.LogInformation("Usuario Admin inicial creado: {Email}", admin.Email);
        }
    }
}
