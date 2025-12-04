
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
// Usings para la Inyección de Dependencias (mantener referencias a tus proyectos)
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Lots;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Application.Services.Users;
using CountrySecure.Application.Services.Visits;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// API básica (mínima) sin dependencias externas opcionales
builder.Services.AddControllers();

// Registro de repositorios y servicios (como estaba)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

//DB
// Obtener la cadena de conexión (puede venir de appsettings o variable de entorno)
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada en appsettings ni en la variable de entorno ConnectionStrings__DefaultConnection.");
}

// Configuración mínima de la BD (requiere paquete Npgsql.EntityFrameworkCore.PostgreSQL)
// Habilita reintentos automáticos para fallos transitorios
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(connStr, npgsqlOptions => npgsqlOptions.EnableRetryOnFailure())
);


var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Loguear si la cadena de conexión viene de variable de entorno (útil para debugging)
var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null;
logger.LogInformation("Connection string 'DefaultConnection' loaded. Override from environment: {FromEnv}", fromEnv);

// Espera proactiva a que la base de datos esté disponible antes de aceptar tráfico
try
{
    await WaitForDatabaseAsync(connStr, logger, timeoutSeconds: 30);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "No se pudo conectar a la base de datos en el tiempo de espera configurado.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    // Página de excepción en desarrollo
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CountrySecureDbContext>();
    // Esto aplicará cualquier migración pendiente a la base de datos.
    dbContext.Database.Migrate();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static async Task WaitForDatabaseAsync(string connectionString, ILogger logger, int timeoutSeconds = 30, CancellationToken ct = default)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    logger.LogInformation("Esperando BD en {Host}:{Port} (no se mostrará la contraseña)", builder.Host, builder.Port);

    var sw = Stopwatch.StartNew();
    int attempt = 0;
    while (sw.Elapsed.TotalSeconds < timeoutSeconds)
    {
        ct.ThrowIfCancellationRequested();
        attempt++;
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            logger.LogInformation("Base de datos disponible. Host: {Host}, Port: {Port}", conn.Host, conn.Port);
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Intento {Attempt}: BD no disponible; reintentando en {Delay}ms...", attempt, Math.Min(1000 * attempt, 5000));
            await Task.Delay(Math.Min(1000 * attempt, 5000), ct);
        }
    }

    throw new TimeoutException($"Tiempo de espera ({timeoutSeconds}s) agotado esperando a la base de datos.");
}