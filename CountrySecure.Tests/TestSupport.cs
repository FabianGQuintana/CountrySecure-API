using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace CountrySecure.Tests;

/// <summary>Base de datos en memoria + usuarios de prueba. Cada test usa su propia base aislada.</summary>
internal static class TestSupport
{
    public static CountrySecureDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<CountrySecureDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CountrySecureDbContext(options);
    }

    public static UnitOfWork NewUnitOfWork(CountrySecureDbContext db)
        => new(db, new UserRepository(db));

    public static ICurrentUserService CurrentUser(Guid userId, string role)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(userId);
        current.Role.Returns(role);
        current.IsAuthenticated.Returns(true);
        current.IsAdmin.Returns(role == "Admin");
        current.GetRequiredUserId().Returns(userId);
        return current;
    }

    public static async Task<User> AddUserAsync(CountrySecureDbContext db, string role = "Resident")
    {
        var n = Random.Shared.Next(1_000_000, 9_999_999);
        var user = new User
        {
            Name = "Test",
            Lastname = role,
            Dni = n,
            Phone = "3794000000",
            Email = $"{role.ToLower()}-{n}@test.local",
            Password = "hash",
            Role = role
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<Amenity> AddAmenityAsync(CountrySecureDbContext db, int capacity)
    {
        var amenity = new Amenity { AmenityName = "Cancha", Description = "Cancha de tenis", Schedules = "8-22", Capacity = capacity };
        db.Amenities.Add(amenity);
        await db.SaveChangesAsync();
        return amenity;
    }
}
