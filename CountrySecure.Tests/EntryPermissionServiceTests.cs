using CountrySecure.Application.Services.EntryPermission;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;

namespace CountrySecure.Tests;

/// <summary>Flujo de portería: vigencia, un solo ingreso, permisos cancelados.</summary>
public class EntryPermissionServiceTests
{
    private static EntryPermissionService ServiceFor(CountrySecureDbContext db, Guid userId, string role)
        => new(new EntryPermissionRepository(db), new VisitRepository(db), new OrderRepository(db),
               TestSupport.NewUnitOfWork(db), TestSupport.CurrentUser(userId, role));

    private static async Task<EntryPermission> AddPermissionAsync(CountrySecureDbContext db, Guid residentId, DateTime validFrom, DateTime validTo)
    {
        var visit = new Visit { NameVisit = "Ana", LastNameVisit = "Gomez", DniVisit = Random.Shared.Next(10_000_000, 99_999_999) };
        db.Visits.Add(visit);
        var permission = new EntryPermission
        {
            QrCodeValue = Guid.NewGuid().ToString("N"),
            PermissionType = PermissionType.Visit,
            EntryPermissionState = PermissionStatus.Pending,
            ValidFrom = validFrom,
            ValidTo = validTo,
            VisitId = visit.Id,
            UserId = residentId
        };
        db.EntryPermissions.Add(permission);
        await db.SaveChangesAsync();
        return permission;
    }

    [Fact]
    public async Task El_guardia_registra_la_entrada_dentro_de_la_vigencia_y_no_puede_repetirla()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(5));
        var service = ServiceFor(db, guard.Id, "Security");

        var result = await service.RegisterCheckInAsync(permission.Id, guard.Id);

        Assert.NotNull(result.EntryTime);
        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterCheckInAsync(permission.Id, guard.Id));
    }

    [Fact]
    public async Task No_se_puede_ingresar_antes_de_que_el_permiso_este_vigente()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(6));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(db, guard.Id, "Security").RegisterCheckInAsync(permission.Id, guard.Id));
    }

    [Fact]
    public async Task No_se_puede_ingresar_con_un_permiso_vencido()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-6), DateTime.UtcNow.AddHours(-2));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(db, guard.Id, "Security").RegisterCheckInAsync(permission.Id, guard.Id));
    }

    [Fact]
    public async Task Un_permiso_cancelado_es_rechazado_en_la_porteria()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(5));

        await ServiceFor(db, resident.Id, "Resident").DeleteAsync(permission.Id, resident.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, guard.Id, "Security").ValidateQrCodeAsync(permission.QrCodeValue));
    }

    [Fact]
    public async Task No_se_puede_cancelar_un_permiso_ya_utilizado()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(5));
        await ServiceFor(db, guard.Id, "Security").RegisterCheckInAsync(permission.Id, guard.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, resident.Id, "Resident").DeleteAsync(permission.Id, resident.Id));
    }

    [Fact]
    public async Task La_salida_exige_haber_registrado_la_entrada()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(5));

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, guard.Id, "Security").RegisterCheckOutAsync(permission.Id, guard.Id));
    }
}
