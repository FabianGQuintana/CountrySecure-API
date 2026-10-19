using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using CountrySecure.API.Authorization;
using CountrySecure.API.Filters;
using CountrySecure.API.Middleware;
using CountrySecure.API.Services;

using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services;
using CountrySecure.Application.Services.AccessControl;
using CountrySecure.Application.Services.Auth;
using CountrySecure.Application.Services.EntryPermission;
using CountrySecure.Application.Services.Lots;
using CountrySecure.Application.Services.Orders;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Application.Services.Request;
using CountrySecure.Application.Services.Turns;
using CountrySecure.Application.Services.Users;
using CountrySecure.Application.Services.Visits;
using CountrySecure.Application.Validators;
using CountrySecure.Domain.Constants;

using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Persistence.Seed;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Infrastructure.Services;

// ======================================================================
//  CONFIGURACIÓN SENSIBLE (no se versiona)
//  En desarrollo se lee de `dotnet user-secrets` (UserSecretsId en el .csproj).
//  Llaves utilizadas:
//    ConnectionStrings:DefaultConnection  -> cadena de conexión a PostgreSQL
//    Jwt:Key                              -> clave HMAC para firmar los JWT (>= 32 caracteres)
//    Seed:Users:{Admin,Resident,Security}:Password -> contraseñas de los usuarios demo (seed)
//  En producción usar variables de entorno: ConnectionStrings__DefaultConnection, Jwt__Key, etc.
//  Ver appsettings.Example.json como plantilla.
// ======================================================================

var builder = WebApplication.CreateBuilder(args);

// ======================================
//   CONTROLLERS + JSON + VALIDACIÓN
// ======================================
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();

// ======================================
//   MANEJO GLOBAL DE ERRORES (ProblemDetails)
// ======================================
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ======================================
//   REPOSITORIOS
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
builder.Services.AddScoped<IAccessLogRepository, AccessLogRepository>();

// ======================================
//   SERVICIOS DE APLICACIÓN
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
builder.Services.AddScoped<IAccessControlService, AccessControlService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IPassTokenService, PassTokenService>();
builder.Services.AddSingleton<IPassDocumentService, PassDocumentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Usuario actual (extraído del JWT)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ======================================
//   CORS
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

// ======================================
//   AUTENTICACIÓN JWT
// ======================================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Falta 'Jwt:Key'. Configurala con: dotnet user-secrets set \"Jwt:Key\" \"<clave>\" (ver appsettings.Example.json).");
}

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role
        };
    });

// ======================================
//   AUTORIZACIÓN
//   FallbackPolicy: todo endpoint sin atributo exige usuario autenticado.
//   Solo login y refresh son [AllowAnonymous].
// ======================================
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(RoleTypes.Admin))
    .AddPolicy(Policies.SecurityStaff, policy => policy.RequireRole(Policies.SecurityStaffRoles));

// ======================================
//   BASE DE DATOS
// ======================================
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Falta 'ConnectionStrings:DefaultConnection'. Configurala con dotnet user-secrets (ver appsettings.Example.json).");
}

builder.Services.AddDbContext<CountrySecureDbContext>(options =>
{
    options.UseNpgsql(connStr, npgsql => npgsql.EnableRetryOnFailure());
    options.EnableSensitiveDataLogging(false);
    options.EnableDetailedErrors(false);
});

// ======================================
//   SWAGGER (con soporte JWT Bearer)
// ======================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CountrySecure API",
        Version = "v1",
        Description = "API de gestión y control de acceso para barrios privados."
    });

    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Pegá el access token obtenido en POST /api/auth/login (sin el prefijo 'Bearer').",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { bearerScheme, Array.Empty<string>() } });
});

// ======================================
//   PIPELINE
// ======================================
var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "CountrySecure API v1"));
}

// En desarrollo el front consume la API por HTTP (http://localhost:5021) sin certificado de confianza.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ======================================
//   SEED (usuarios demo: Admin, Resident, Security)
// ======================================
// Corre en segundo plano una vez iniciada la app, para no bloquear el arranque
// si la base todavía no está disponible o las migraciones no se aplicaron.
app.Lifetime.ApplicationStarted.Register(() => _ = Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CountrySecureDbContext>();
        await DbSeeder.SeedAsync(db, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo ejecutar el seed de usuarios demo. ¿Está la BD levantada y aplicaste las migraciones (dotnet ef database update)?");
    }
}));

app.Run();
