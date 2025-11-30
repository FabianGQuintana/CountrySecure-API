using AutoMapper;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.MappingProfiles;
using CountrySecure.Application.Services.Properties;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Services.Users;
using FluentValidation;
using CountrySecure.Application.Validators;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();


builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



// 3. REGISTRO DE DEPENDENCIAS DE PROPERTY Y DE INFRAESTRUCTURA

// Registrar el Repositorio de Property (Contrato <-> Implementaci�n)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// Registrar la Clase Base del Repositorio Gen�rico (Si la usas, aunque la anterior es suficiente)
// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>)); 

// Registrar el Servicio de Property (L�gica de Negocio)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

// Registrar la Unidad de Trabajo (DEBES HACER ESTO)
// Asumo que tienes una interfaz IUnitOfWork y una implementaci�n UnitOfWork en Infrastructure/Persistence.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// PostgreSQL DbContext
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);


// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

// OpenAPI
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