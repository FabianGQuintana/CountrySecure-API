using CountrySecure.Domain.Constants;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CountrySecure.Infrastructure.Persistence.Seed
{
    /// <summary>
    /// Garantiza un usuario de prueba por rol (Admin, Resident, Security).
    /// Es idempotente: crea el usuario si falta y, si ya existe, re-sincroniza su contraseña
    /// con la configurada, para que las credenciales de demo siempre funcionen.
    ///
    /// Las contraseñas se leen de configuración (user-secrets / variables de entorno):
    ///   Seed:Users:Admin:Password, Seed:Users:Resident:Password, Seed:Users:Security:Password
    /// Opcionalmente se puede cambiar el email con Seed:Users:{Rol}:Email.
    /// </summary>
    public static class DbSeeder
    {
        private record DemoUser(string Role, string DefaultEmail, string Name, string Lastname, int Dni, string Phone);

        private static readonly DemoUser[] DemoUsers =
        [
            new(RoleTypes.Admin,    "admin@countrysecure.local",     "Admin",     "Demo", 1000001, "3794000001"),
            new(RoleTypes.Resident, "residente@countrysecure.local", "Residente", "Demo", 1000002, "3794000002"),
            new(RoleTypes.Security, "seguridad@countrysecure.local", "Guardia",   "Demo", 1000003, "3794000003"),
        ];

        public static async Task SeedAsync(CountrySecureDbContext context, IConfiguration configuration, ILogger logger)
        {
            User? demoResident = null;

            foreach (var demo in DemoUsers)
            {
                var password = configuration[$"Seed:Users:{demo.Role}:Password"];
                if (string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning("Seed:Users:{Role}:Password no configurado. Se omite el usuario demo {Role}.", demo.Role, demo.Role);
                    continue;
                }

                var email = (configuration[$"Seed:Users:{demo.Role}:Email"] ?? demo.DefaultEmail).Trim().ToLowerInvariant();
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Name = demo.Name,
                        Lastname = demo.Lastname,
                        Dni = demo.Dni,
                        Phone = demo.Phone,
                        Email = email,
                        Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10),
                        Role = demo.Role,
                        CreatedBy = "seed"
                    };
                    context.Users.Add(user);
                    logger.LogInformation("Usuario demo creado: {Email} ({Role})", email, demo.Role);
                }
                else
                {
                    // Re-sincroniza credenciales y estado para que el acceso de demo esté garantizado
                    if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);

                    user.Role = demo.Role;
                    user.DeletedAt = null;
                }

                if (demo.Role == RoleTypes.Resident)
                    demoResident = user;
            }

            await context.SaveChangesAsync();

            if (demoResident != null)
                await EnsureDemoPropertyAsync(context, demoResident, logger);
        }

        /// <summary>
        /// Le asigna al residente demo una propiedad (y su lote) para que el perfil no aparezca vacío.
        /// </summary>
        private static async Task EnsureDemoPropertyAsync(CountrySecureDbContext context, User resident, ILogger logger)
        {
            if (await context.Properties.AnyAsync(p => p.UserId == resident.Id))
                return;

            const string lotName = "Lote Demo";
            var lot = await context.Lots.FirstOrDefaultAsync(l => l.LotName == lotName);
            if (lot == null)
            {
                lot = new Lot
                {
                    Id = Guid.NewGuid(),
                    LotName = lotName,
                    BlockName = "Demo",
                    LotState = LotStatus.Available,
                    CreatedBy = "seed"
                };
                context.Lots.Add(lot);
            }

            const string street = "Calle Demo";
            var property = await context.Properties.FirstOrDefaultAsync(p => p.Street == street && p.PropertyNumber == 1);
            if (property == null)
            {
                property = new Property
                {
                    Id = Guid.NewGuid(),
                    Street = street,
                    PropertyNumber = 1,
                    PropertyStatus = PropertyStatus.Occupied,
                    LotId = lot.Id,
                    CreatedBy = "seed"
                };
                context.Properties.Add(property);
            }

            property.UserId = resident.Id;
            await context.SaveChangesAsync();
            logger.LogInformation("Propiedad demo asignada a {Email}", resident.Email);
        }
    }
}
