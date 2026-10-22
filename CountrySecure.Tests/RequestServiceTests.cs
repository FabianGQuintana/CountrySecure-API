using CountrySecure.Application.DTOs.Request;
using CountrySecure.Application.Services.Request;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Enums;
using CountrySecure.Domain.Exceptions;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Repositories;

namespace CountrySecure.Tests;

/// <summary>Solicitudes de servicio: el residente las crea; solo la administración las resuelve o rechaza.</summary>
public class RequestServiceTests
{
    private static RequestService ServiceFor(CountrySecureDbContext db, Guid userId, string role)
        => new(new RequestRepository(db), new OrderRepository(db), TestSupport.NewUnitOfWork(db), TestSupport.CurrentUser(userId, role));

    private static async Task<Request> AddPendingRequestAsync(CountrySecureDbContext db, User resident)
    {
        var order = new Order { SupplierName = "Proveedor", Description = "Jardinería", Category = OrderCategory.Gardening };
        db.Orders.Add(order);
        var request = new Request { Details = "Cortar el pasto", Location = "Patio", Order = order, User = resident, RequestStatus = RequestStatus.Pending };
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    [Fact]
    public async Task El_residente_no_puede_marcar_su_solicitud_como_resuelta()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var request = await AddPendingRequestAsync(db, resident);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            ServiceFor(db, resident.Id, "Resident").UpdateAsync(request.Id, new UpdateRequestDto { Status = RequestStatus.Completed }, resident.Id));
    }

    [Fact]
    public async Task El_residente_puede_corregir_el_detalle()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var request = await AddPendingRequestAsync(db, resident);

        var result = await ServiceFor(db, resident.Id, "Resident").UpdateAsync(request.Id, new UpdateRequestDto { Details = "Cortar el pasto y podar" }, resident.Id);

        Assert.Equal("Cortar el pasto y podar", result.Details);
    }

    [Fact]
    public async Task El_admin_resuelve_una_solicitud_pendiente_una_sola_vez()
    {
        using var db = TestSupport.NewDb();
        var resident = await TestSupport.AddUserAsync(db);
        var admin = await TestSupport.AddUserAsync(db, "Admin");
        var request = await AddPendingRequestAsync(db, resident);
        var service = ServiceFor(db, admin.Id, "Admin");

        var result = await service.UpdateAsync(request.Id, new UpdateRequestDto { Status = RequestStatus.Completed }, admin.Id);
        Assert.Equal(RequestStatus.Completed, result.Status);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UpdateAsync(request.Id, new UpdateRequestDto { Status = RequestStatus.Cancelled }, admin.Id));
    }
}
