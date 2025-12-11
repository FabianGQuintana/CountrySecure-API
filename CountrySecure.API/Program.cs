using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation;

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
using CountrySecure.Application.Services.EntryPermission;

using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Infrastructure.Services;
using CountrySecure.Infrastructure.Utils;

using CountrySecure.Application.Validators;
using CountrySecure.API.Filters;
using CountrySecure.Application.Services;
using CountrySecure.Application.Services.Turns;
using System.Security.Claims;  // si tu ValidationFilter está aquí (ajustalo según tu proyecto)


var builder = WebApplication.CreateBuilder(args);

//
// ======================================
//       CONFIGURACIÓN DE SERVICIOS
// ======================================
//

// Controllers + JSON + Validation Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});


// ======================================
//          REPOSITORIOS
// ======================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<ITurnRepository, TurnRepository>();
builder.Services.AddScoped<IEntryPermissionRepository, EntryPermissionRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();


// ======================================
//          SERVICIOS DOMINIO
// ======================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<ITurnService, TurnService>();
builder.Services.AddScoped<IEntryPermissionService, EntryPermissionService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRequestService, RequestService>();


// ======================================
//          SERVICIOS AGREGADOS
// ======================================
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Unity of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JwtUtils
builder.Services.AddSingleton<JwtUtils>();

// Filters
builder.Services.AddScoped<ValidationFilter>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();


// ======================================
//                 CORS
// ======================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});


//
// ======================================
//            AUTENTICACIÓN JWT
// ======================================
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
            ),

            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();


//
// ======================================
//          CONFIG BASE DE DATOS
// ======================================
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Cadena de conexión 'DefaultConnection' no encontrada."
    );
}

// DbContext con reintentos
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
{
    options.UseNpgsql(connStr, npgsql => npgsql.EnableRetryOnFailure());
    options.EnableSensitiveDataLogging(false);
    options.EnableDetailedErrors(false);
    options.LogTo(_ => { }, LogLevel.None); // desactiva logs SQL
});


//
// ======================================
//          BUILD + PIPELINE
// ======================================
var app = builder.Build();

// var logger = app.Services.GetRequiredService<ILogger<Program>>();

// var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") != null;
// logger.LogInformation("Connection string cargada. Override desde ENV: {FromEnv}", fromEnv);


// Esperar BD antes de iniciar
// try
// {
//     await WaitForDatabaseAsync(connStr, logger, timeoutSeconds: 30);
// }
// catch (Exception ex)
// {
//     logger.LogCritical(ex, "No se pudo conectar a la base de datos.");
//     throw;
// }


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


//
// ======================================
//         FUNCIÓN WAIT-FOR-DB
// ======================================
// static async Task WaitForDatabaseAsync(string connectionString, ILogger logger, int timeoutSeconds = 30, CancellationToken ct = default)
// {
//     var builder = new NpgsqlConnectionStringBuilder(connectionString);
//     logger.LogInformation("Esperando BD en {Host}:{Port}", builder.Host, builder.Port);

//     var sw = Stopwatch.StartNew();
//     int attempt = 0;

//     while (sw.Elapsed.TotalSeconds < timeoutSeconds)
//     {
//         ct.ThrowIfCancellationRequested();
//         attempt++;

//         try
//         {
//             await using var conn = new NpgsqlConnection(connectionString);
//             await conn.OpenAsync(ct);

//             logger.LogInformation("Base de datos DISPONIBLE en {Host}:{Port}", conn.Host, conn.Port);
//             return;
//         }
//         catch (Exception ex)
//         {
//             int delay = Math.Min(1000 * attempt, 5000);
//             logger.LogWarning(ex, "Intento {Attempt}: BD no disponible; reintentando en {Delay} ms...", attempt, delay);
//             await Task.Delay(delay, ct);
//         }
//     }

//     throw new TimeoutException($"Tiempo de espera agotado esperando la BD ({timeoutSeconds}s).");
// }
