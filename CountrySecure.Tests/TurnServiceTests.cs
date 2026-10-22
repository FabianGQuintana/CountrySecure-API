using CountrySecure.Application.DTOs.Turns;
using CountrySecure.Application.Services.Turns;
using CountrySecure.Domain.Exceptions;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;

namespace CountrySecure.Tests;

public class TurnServiceTests
{
    private static readonly DateTime Start = DateTime.UtcNow.AddDays(1);

    private static TurnService ServiceFor(CountrySecureDbContext db, Guid userId, string role = "Resident")
        => new(new TurnRepository(db), new AmenityRepository(db), TestSupport.NewUnitOfWork(db), TestSupport.CurrentUser(userId, role));

    private static CreateTurnDto Dto(Guid amenityId, DateTime start, TimeSpan? length = null)
        => new() { AmenityId = amenityId, StartTime = start, EndTime = start + (length ?? TimeSpan.FromHours(1)) };

    [Fact]
    public async Task No_se_puede_reservar_en_el_pasado()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(db, user.Id).CreateAsync(Dto(amenity.Id, DateTime.UtcNow.AddHours(-3)), user.Id));
    }

    [Fact]
    public async Task El_inicio_debe_ser_anterior_al_fin()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            ServiceFor(db, user.Id).CreateAsync(Dto(amenity.Id, Start, TimeSpan.Zero), user.Id));
    }

    [Fact]
    public async Task Capacidad_1_es_uso_exclusivo_y_el_segundo_usuario_recibe_conflicto()
    {
        using var db = TestSupport.NewDb();
        var a = await TestSupport.AddUserAsync(db);
        var b = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        await ServiceFor(db, a.Id).CreateAsync(Dto(amenity.Id, Start), a.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, b.Id).CreateAsync(Dto(amenity.Id, Start.AddMinutes(30)), b.Id));
    }

    [Fact]
    public async Task Un_mismo_usuario_no_puede_solapar_sus_propias_reservas_aunque_haya_capacidad()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 5);

        await ServiceFor(db, user.Id).CreateAsync(Dto(amenity.Id, Start), user.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, user.Id).CreateAsync(Dto(amenity.Id, Start.AddMinutes(15)), user.Id));
    }

    [Fact]
    public async Task Se_respeta_la_capacidad_del_amenity()
    {
        using var db = TestSupport.NewDb();
        var a = await TestSupport.AddUserAsync(db);
        var b = await TestSupport.AddUserAsync(db);
        var c = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 2);

        await ServiceFor(db, a.Id).CreateAsync(Dto(amenity.Id, Start), a.Id);
        await ServiceFor(db, b.Id).CreateAsync(Dto(amenity.Id, Start), b.Id);

        await Assert.ThrowsAsync<ConflictException>(() =>
            ServiceFor(db, c.Id).CreateAsync(Dto(amenity.Id, Start), c.Id));
    }

    [Fact]
    public async Task Turnos_consecutivos_no_se_consideran_solapados()
    {
        using var db = TestSupport.NewDb();
        var a = await TestSupport.AddUserAsync(db);
        var b = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        await ServiceFor(db, a.Id).CreateAsync(Dto(amenity.Id, Start), a.Id);
        var second = await ServiceFor(db, b.Id).CreateAsync(Dto(amenity.Id, Start.AddHours(1)), b.Id);

        Assert.NotEqual(Guid.Empty, second.Id);
    }

    [Fact]
    public async Task Cancelar_una_reserva_libera_el_horario()
    {
        using var db = TestSupport.NewDb();
        var a = await TestSupport.AddUserAsync(db);
        var b = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        var turn = await ServiceFor(db, a.Id).CreateAsync(Dto(amenity.Id, Start), a.Id);
        await ServiceFor(db, a.Id).DeleteAsync(turn.Id, a.Id);

        var rebooked = await ServiceFor(db, b.Id).CreateAsync(Dto(amenity.Id, Start), b.Id);
        Assert.NotEqual(turn.Id, rebooked.Id);
    }

    [Fact]
    public async Task Un_residente_no_puede_cancelar_la_reserva_de_otro()
    {
        using var db = TestSupport.NewDb();
        var owner = await TestSupport.AddUserAsync(db);
        var other = await TestSupport.AddUserAsync(db);
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        var turn = await ServiceFor(db, owner.Id).CreateAsync(Dto(amenity.Id, Start), owner.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            ServiceFor(db, other.Id).DeleteAsync(turn.Id, other.Id));
    }

    [Fact]
    public async Task Un_admin_si_puede_cancelar_la_reserva_de_un_residente()
    {
        using var db = TestSupport.NewDb();
        var owner = await TestSupport.AddUserAsync(db);
        var admin = await TestSupport.AddUserAsync(db, "Admin");
        var amenity = await TestSupport.AddAmenityAsync(db, 1);

        var turn = await ServiceFor(db, owner.Id).CreateAsync(Dto(amenity.Id, Start), owner.Id);
        var cancelled = await ServiceFor(db, admin.Id, "Admin").DeleteAsync(turn.Id, admin.Id);

        Assert.Equal(turn.Id, cancelled.Id);
    }
}
