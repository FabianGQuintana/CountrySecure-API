using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

// Inyección de dependencias
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;

using CountrySecure.Application.Services.Lots;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Application.Services.Users;
using CountrySecure.Application.Services.Visits;
using CountrySecure.Application.Services.Orders;
using CountrySecure.Application.Services.Request;

using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Infrastructure.Services;



var builder = WebApplication.CreateBuilder(args);

//
// ======================================
//       CONFIGURACIÓN DE SERVICIOS
// ======================================
//

// Controllers + JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();

// Servicios de dominio
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRequestService, RequestService>();

// Servicios agregados del segundo Program
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// ---------------------------
// 3) CORS (IMPORTANTE!)
// ---------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin(); // Desarrollo
    });
});


//
// ======================================
//            AUTENTICACIÓN JWT
// ======================================
//

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


//
// ======================================
//          CONFIG BASE DE DATOS
// ======================================
//

// Obtener ConnectionString del appsettings o del entorno
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Cadena de conexión 'DefaultConnection' no encontrada."
    );
}

// DbContext con reintentos ante fallos transitorios
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(connStr, npgsql => npgsql.EnableRetryOnFailure())
);


//
// ======================================
//          BUILD + PIPELINE
// ======================================
//

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null;
logger.LogInformation("Connection string cargada. Override desde ENV: {FromEnv}", fromEnv);

// Esperar a BD antes de iniciar
try
{
    await WaitForDatabaseAsync(connStr, logger, timeoutSeconds: 30);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "No se pudo conectar a la base de datos.");
    throw;
}

// Migraciones automáticas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CountrySecureDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");  // <--- HABILITA CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


//
// ======================================
//         FUNCIÓN WAIT-FOR-DB
// ======================================
//

static async Task WaitForDatabaseAsync(string connectionString, ILogger logger, int timeoutSeconds = 30, CancellationToken ct = default)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    logger.LogInformation("Esperando BD en {Host}:{Port}", builder.Host, builder.Port);

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

            logger.LogInformation("Base de datos DISPONIBLE en {Host}:{Port}", conn.Host, conn.Port);
            return;
        }
        catch (Exception ex)
        {
            int delay = Math.Min(1000 * attempt, 5000);
            logger.LogWarning(ex, "Intento {Attempt}: BD no disponible; reintentando en {Delay} ms...", attempt, delay);
            await Task.Delay(delay, ct);
        }
    }

    throw new TimeoutException($"Tiempo de espera agotado esperando la BD ({timeoutSeconds}s).");
}
