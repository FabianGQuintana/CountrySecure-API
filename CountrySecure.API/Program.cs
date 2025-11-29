using Microsoft.EntityFrameworkCore;

using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Users;
using FluentValidation;
using CountrySecure.Application.Validators;
using FluentValidation.AspNetCore;



var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// PostgreSQL DbContext
builder.Services.AddDbContext<CountrySecureDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();


// Unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// Services
builder.Services.AddScoped<IUserService, UserService>();


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
