using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.MappingProfiles;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



// 3. REGISTRO DE DEPENDENCIAS DE PROPERTY Y DE INFRAESTRUCTURA

// Registrar el Repositorio de Property (Contrato <-> Implementación)
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Registrar la Clase Base del Repositorio Genérico (Si la usas, aunque la anterior es suficiente)
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>)); 

// Registrar el Servicio de Property (Lógica de Negocio)
builder.Services.AddScoped<IPropertyService, PropertyService>();

// Registrar la Unidad de Trabajo (DEBES HACER ESTO)
// Asumo que tienes una interfaz IUnitOfWork y una implementación UnitOfWork en Infrastructure/Persistence.
// builder.Services.AddScoped<CountrySecure.Application.Interfaces.UnitOfWork.IUnitOfWork, CountrySecure.Infrastructure.Persistence.UnitOfWork>();


// PostgreSQL DbContext
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();