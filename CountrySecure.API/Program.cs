using System;
using Microsoft.EntityFrameworkCore;
// Usings para la Inyección de Dependencias (mantener referencias a tus proyectos)
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Lots;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Application.Services.Users;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// API básica (mínima) sin dependencias externas opcionales
builder.Services.AddControllers();

// Registro de repositorios y servicios (como estaba)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<ILotService, LotService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configuración mínima de la BD (requiere paquete Npgsql.EntityFrameworkCore.PostgreSQL)
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Página de excepción en desarrollo
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();