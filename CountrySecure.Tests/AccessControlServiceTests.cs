using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Application.Services.AccessControl;
using CountrySecure.Application.Services.EntryPermission;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;
using CountrySecure.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CountrySecure.Tests;

/// <summary>Escaneo de pases en la guardia: validación secuencial, check-in automático y auditoría.</summary>
public class AccessControlServiceTests
{
    private readonly PassTokenService _tokens = TestSupport.Tokens();

    private AccessControlService ServiceFor(CountrySecureDbContext db, Guid userId, string role = "Security")
        => new(new EntryPermissionRepository(db), new AccessLogRepository(db), new NotificationRepository(db), _tokens,
               TestSupport.NewUnitOfWork(db), TestSupport.CurrentUser(userId, role));

    private async Task<EntryPermission> AddPermissionAsync(
        CountrySecureDbContext db, Guid residentId,
        DateTime? validFrom = null, DateTime? validTo = null,
        PassType passType = PassType.SingleEntry, bool signed = true)
    {
        var visit = new Visit { NameVisit = "Ana", LastNameVisit = "Gomez", DniVisit = Random.Shared.Next(10_000_000, 99_999_999) };
        db.Visits.Add(visit);

        var permission = new EntryPermission
        {
            QrCodeValue = string.Empty,
            PermissionType = PermissionType.Visit,
            EntryPermissionState = PermissionStatus.Pending,
            ValidFrom = validFrom ?? DateTime.UtcNow.AddHours(-1),
            ValidTo = validTo ?? DateTime.UtcNow.AddHours(5),
            VisitId = visit.Id,
            UserId = residentId,
            PassType = passType,
            CompanionsCount = 2,
            VehicleType = VehicleType.Car,
            VehiclePlate = "AB123CD"
        };
        permission.QrCodeValue = signed ? _tokens.Create(permission.Id) : permission.Id.ToString();

        db.EntryPermissions.Add(permission);
        await db.SaveChangesAsync();
        return permission;
    }

    private static ScanRequestDto Scan(string token, ScanAction action = ScanAction.CheckIn) => new() { Token = token, Action = action };

    // ---------------------------------------------------------------- éxito

    [Fact]
    public async Task Un_pase_valido_registra_el_ingreso_y_devuelve_la_ficha_completa()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.True(result.Success);
        Assert.Equal(ScanOutcome.CheckIn, result.Outcome);
        Assert.Equal(ScanReason.None, result.Reason);
        Assert.Equal("Ana Gomez", result.Pass!.VisitorName);
        Assert.Equal(2, result.Pass.CompanionsCount);
        Assert.Equal("AB123CD", result.Pass.VehiclePlate);

        var stored = await db.EntryPermissions.SingleAsync();
        Assert.NotNull(stored.EntryTime);
        Assert.Equal(guard.Id, stored.CheckInGuardId);
        Assert.Equal(PermissionStatus.Completed, stored.EntryPermissionState);
    }

    [Fact]
    public async Task Todo_escaneo_deja_un_registro_de_auditoria_con_el_guardia_y_el_resultado()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        var service = ServiceFor(db, guard.Id);

        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id); // rechazado: ya utilizado

        var logs = await db.AccessLogs.OrderBy(l => l.ScannedAt).ToListAsync();
        Assert.Equal(2, logs.Count);
        Assert.All(logs, l => Assert.Equal(guard.Id, l.GuardId));
        Assert.Equal(ScanOutcome.CheckIn, logs[0].Outcome);
        Assert.Equal(ScanOutcome.Rejected, logs[1].Outcome);
        Assert.Equal(ScanReason.AlreadyUsed, logs[1].Reason);
    }

    // ------------------------------------------------------------- rechazos

    [Fact]
    public async Task Un_pase_de_un_solo_ingreso_no_se_puede_usar_dos_veces()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        var service = ServiceFor(db, guard.Id);

        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        var second = await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.False(second.Success);
        Assert.Equal(ScanReason.AlreadyUsed, second.Reason);
        Assert.Equal("Invitación ya utilizada.", second.Message);
    }

    [Fact]
    public async Task Un_token_con_firma_falsa_se_rechaza_y_se_audita()
    {
        using var db = TestSupport.NewDb();
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var forged = $"{Guid.NewGuid():N}.AAAAAAAAAAAAAAAAAAAAAA";

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(forged), guard.Id);

        Assert.False(result.Success);
        Assert.Equal(ScanReason.InvalidToken, result.Reason);
        Assert.Null(result.Pass);
        var log = await db.AccessLogs.SingleAsync();
        Assert.Null(log.PermissionId);
        Assert.Equal(ScanReason.InvalidToken, log.Reason);
    }

    [Fact]
    public async Task Un_token_bien_firmado_de_un_permiso_inexistente_no_se_encuentra()
    {
        using var db = TestSupport.NewDb();
        var guard = await TestSupport.AddUserAsync(db, "Security");

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(_tokens.Create(Guid.NewGuid())), guard.Id);

        Assert.False(result.Success);
        Assert.Equal(ScanReason.NotFound, result.Reason);
    }

    [Fact]
    public async Task Un_pase_cancelado_por_el_residente_se_rechaza()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        permission.SoftDelete();
        permission.EntryPermissionState = PermissionStatus.Cancelled;
        await db.SaveChangesAsync();

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.False(result.Success);
        Assert.Equal(ScanReason.Cancelled, result.Reason);
        Assert.Equal("Pase cancelado por el residente.", result.Message);
    }

    [Fact]
    public async Task Fuera_de_la_ventana_horaria_se_rechaza_antes_y_despues()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var future = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(6));
        var past = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddHours(-6), DateTime.UtcNow.AddHours(-2));
        var service = ServiceFor(db, guard.Id);

        var early = await service.ScanAsync(Scan(future.QrCodeValue), guard.Id);
        var late = await service.ScanAsync(Scan(past.QrCodeValue), guard.Id);

        Assert.Equal(ScanReason.NotYetValid, early.Reason);
        Assert.Equal(ScanReason.Expired, late.Reason);
        Assert.Contains("Fuera de horario permitido", early.Message);
        Assert.Contains("Fuera de horario permitido", late.Message);
    }

    [Fact]
    public async Task Un_visitante_bloqueado_no_ingresa_aunque_tenga_un_pase_vigente()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        var visit = await db.Visits.SingleAsync();
        visit.IsBlocked = true;
        await db.SaveChangesAsync();

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.False(result.Success);
        Assert.Equal(ScanReason.Blocked, result.Reason);
        Assert.Null((await db.EntryPermissions.SingleAsync()).EntryTime);
    }

    [Fact]
    public async Task Un_residente_bloqueado_no_puede_recibir_visitas()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        resident.IsBlocked = true;
        await db.SaveChangesAsync();

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.False(result.Success);
        Assert.Equal(ScanReason.Blocked, result.Reason);
    }

    // ------------------------------------------------------ salida y múltiples

    [Fact]
    public async Task La_salida_exige_haber_ingresado_y_se_registra_con_action_checkout()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        var service = ServiceFor(db, guard.Id);

        var beforeEntry = await service.ScanAsync(Scan(permission.QrCodeValue, ScanAction.CheckOut), guard.Id);
        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        var exit = await service.ScanAsync(Scan(permission.QrCodeValue, ScanAction.CheckOut), guard.Id);

        Assert.Equal(ScanReason.NotInside, beforeEntry.Reason);
        Assert.True(exit.Success);
        Assert.Equal(ScanOutcome.CheckOut, exit.Outcome);
        Assert.NotNull((await db.EntryPermissions.SingleAsync()).DepartureTime);
    }

    [Fact]
    public async Task El_pase_de_multiples_ingresos_permite_salir_y_volver_a_entrar_pero_no_entrar_dos_veces_seguidas()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id, passType: PassType.MultipleEntries);
        var service = ServiceFor(db, guard.Id);

        var first = await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        var whileInside = await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        await service.ScanAsync(Scan(permission.QrCodeValue, ScanAction.CheckOut), guard.Id);
        var secondEntry = await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);

        Assert.True(first.Success);
        Assert.Equal(ScanReason.AlreadyInside, whileInside.Reason);
        Assert.True(secondEntry.Success);
        Assert.Equal(PermissionStatus.Pending, (await db.EntryPermissions.SingleAsync()).EntryPermissionState);
        Assert.Equal(4, await db.AccessLogs.CountAsync());
    }

    // ------------------------------------------------------ offline y legado

    [Fact]
    public async Task Un_escaneo_offline_conserva_la_hora_de_la_guardia_y_nunca_una_hora_futura()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var first = await AddPermissionAsync(db, resident.Id);
        var second = await AddPermissionAsync(db, resident.Id);
        var service = ServiceFor(db, guard.Id);
        var scannedAt = DateTime.UtcNow.AddMinutes(-20);

        var offline = await service.ScanAsync(new ScanRequestDto { Token = first.QrCodeValue, Offline = true, ScannedAt = scannedAt }, guard.Id);
        var future = await service.ScanAsync(new ScanRequestDto { Token = second.QrCodeValue, Offline = true, ScannedAt = DateTime.UtcNow.AddDays(1) }, guard.Id);

        Assert.Equal(scannedAt, offline.ScannedAt, TimeSpan.FromSeconds(1));
        Assert.True(future.ScannedAt <= DateTime.UtcNow);
        Assert.True((await db.AccessLogs.OrderBy(l => l.ScannedAt).FirstAsync()).Offline);
    }

    [Fact]
    public async Task Los_pases_emitidos_antes_de_las_firmas_siguen_funcionando()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var legacy = await AddPermissionAsync(db, resident.Id, signed: false);

        var result = await ServiceFor(db, guard.Id).ScanAsync(Scan(legacy.QrCodeValue), guard.Id);

        Assert.True(result.Success);
    }

    // ------------------------------------------------- avisos y sincronización

    [Fact]
    public async Task El_ingreso_y_la_salida_avisan_al_residente_pero_un_rechazo_no()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var permission = await AddPermissionAsync(db, resident.Id);
        var service = ServiceFor(db, guard.Id);

        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id);
        await service.ScanAsync(Scan(permission.QrCodeValue), guard.Id); // rechazado
        await service.ScanAsync(Scan(permission.QrCodeValue, ScanAction.CheckOut), guard.Id);

        var notes = await db.Notifications.OrderBy(n => n.CreatedAt).ToListAsync();
        Assert.Equal(2, notes.Count);
        Assert.All(notes, n => Assert.Equal(resident.Id, n.UserId));
        Assert.Equal("Tu visita Ana Gomez ha ingresado al barrio.", notes[0].Message);
        Assert.Equal("VisitCheckOut", notes[1].Type);
    }

    [Fact]
    public async Task La_sincronizacion_trae_los_pases_activos_con_token_y_marca_a_los_bloqueados()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var guard = await TestSupport.AddUserAsync(db, "Security");
        var ok = await AddPermissionAsync(db, resident.Id);
        var cancelled = await AddPermissionAsync(db, resident.Id);
        cancelled.EntryPermissionState = PermissionStatus.Cancelled;
        var farFuture = await AddPermissionAsync(db, resident.Id, DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(4));
        var blocked = await AddPermissionAsync(db, resident.Id);
        (await db.Visits.FindAsync(blocked.VisitId))!.IsBlocked = true;
        await db.SaveChangesAsync();

        var sync = await ServiceFor(db, guard.Id, "Security").GetSyncAsync();

        Assert.Equal(2, sync.Items.Count);
        Assert.Contains(sync.Items, i => i.Token == ok.QrCodeValue && !i.Blocked);
        Assert.Contains(sync.Items, i => i.Token == blocked.QrCodeValue && i.Blocked);
        Assert.DoesNotContain(sync.Items, i => i.Token == cancelled.QrCodeValue || i.Token == farFuture.QrCodeValue);
    }

    // --------------------------------------------------------------- creación

    [Fact]
    public async Task Al_crear_un_permiso_el_QR_es_un_token_firmado_y_la_patente_se_normaliza()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var visit = new Visit { NameVisit = "Ana", LastNameVisit = "Gomez", DniVisit = 30111222 };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();

        var permissions = new EntryPermissionService(
            new EntryPermissionRepository(db), new VisitRepository(db), new OrderRepository(db),
            TestSupport.NewUnitOfWork(db), TestSupport.CurrentUser(resident.Id, "Resident"), _tokens);

        var created = await permissions.CreateAsync(new CreateEntryPermissionDto
        {
            PermissionType = PermissionType.Visit,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddHours(4),
            VisitId = visit.Id,
            UserId = resident.Id,
            CompanionsCount = 3,
            VehicleType = VehicleType.Car,
            VehiclePlate = "ab 123-cd",
            VehicleModel = "  Fiat Cronos ",
            PassType = PassType.MultipleEntries
        }, resident.Id);

        Assert.True(_tokens.TryValidate(created.QrCodeValue, out var id));
        Assert.Equal(created.Id, id);
        Assert.Equal("AB123CD", created.VehiclePlate);
        Assert.Equal("Fiat Cronos", created.VehicleModel);
        Assert.Equal(3, created.CompanionsCount);
        Assert.Equal(PassType.MultipleEntries, created.PassType);
    }
}
