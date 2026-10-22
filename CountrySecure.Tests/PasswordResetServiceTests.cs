using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Auth;
using CountrySecure.Domain.Entities;
using CountrySecure.Infrastructure.Persistence;
using CountrySecure.Infrastructure.Services;
using NSubstitute;

namespace CountrySecure.Tests;

/// <summary>Recuperación de contraseña: enlace de un solo uso, con vencimiento y sin revelar si la cuenta existe.</summary>
public class PasswordResetServiceTests
{
    private const string ResetPage = "http://localhost:3000/reset-password";

    private static (PasswordResetService service, IEmailSender email) Build(CountrySecureDbContext db)
    {
        var email = Substitute.For<IEmailSender>();
        var service = new PasswordResetService(TestSupport.NewUnitOfWork(db), new BcryptPasswordHasher(), email);
        return (service, email);
    }

    /// <summary>Extrae el token del enlace que se habría enviado por email.</summary>
    private static string SentToken(IEmailSender email)
    {
        var call = email.ReceivedCalls().Last();
        var text = (string)call.GetArguments()[3]!;
        var start = text.IndexOf("?token=", StringComparison.Ordinal) + "?token=".Length;
        var end = text.IndexOf('\n', start);
        return Uri.UnescapeDataString(text[start..end]);
    }

    [Fact]
    public async Task Envia_un_enlace_y_guarda_solo_el_hash()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var (service, email) = Build(db);

        await service.RequestResetAsync(new ForgotPasswordDto { Email = user.Email.ToUpper() }, ResetPage);

        var token = SentToken(email);
        var stored = Assert.Single(db.PasswordResetTokens);
        Assert.Equal(PasswordResetService.Hash(token), stored.TokenHash);
        Assert.DoesNotContain(token, stored.TokenHash);
        await email.Received(1).SendAsync(user.Email, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Email_inexistente_no_envia_nada_ni_falla()
    {
        using var db = TestSupport.NewDb();
        var (service, email) = Build(db);

        await service.RequestResetAsync(new ForgotPasswordDto { Email = "nadie@test.local" }, ResetPage);

        Assert.Empty(db.PasswordResetTokens);
        Assert.Empty(email.ReceivedCalls());
    }

    [Fact]
    public async Task Cambia_la_contrasena_una_sola_vez_y_cierra_las_sesiones()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        db.RefreshTokens.Add(new RefreshToken { Token = "sesion-abierta", UserId = user.Id, Expires = DateTime.UtcNow.AddDays(7) });
        await db.SaveChangesAsync();
        var (service, email) = Build(db);

        await service.RequestResetAsync(new ForgotPasswordDto { Email = user.Email }, ResetPage);
        var token = SentToken(email);

        var first = await service.ResetPasswordAsync(new ResetPasswordDto { Token = token, NewPassword = "Nueva-Clave1" });
        var second = await service.ResetPasswordAsync(new ResetPasswordDto { Token = token, NewPassword = "Otra-Clave2" });

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.True(new BcryptPasswordHasher().Verify("Nueva-Clave1", user.Password));
        Assert.All(db.RefreshTokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public async Task Un_enlace_vencido_no_sirve()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var (service, email) = Build(db);

        await service.RequestResetAsync(new ForgotPasswordDto { Email = user.Email }, ResetPage);
        var token = SentToken(email);
        db.PasswordResetTokens.Single().Expires = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var result = await service.ResetPasswordAsync(new ResetPasswordDto { Token = token, NewPassword = "Nueva-Clave1" });

        Assert.False(result.Success);
        Assert.Equal("hash", user.Password);
    }

    [Fact]
    public async Task Pedir_un_enlace_nuevo_invalida_el_anterior_y_limita_los_pedidos()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var (service, email) = Build(db);
        var dto = new ForgotPasswordDto { Email = user.Email };

        await service.RequestResetAsync(dto, ResetPage);
        var first = SentToken(email);
        await service.RequestResetAsync(dto, ResetPage);
        await service.RequestResetAsync(dto, ResetPage);
        await service.RequestResetAsync(dto, ResetPage); // supera el límite: no se envía

        Assert.Equal(3, email.ReceivedCalls().Count());
        var result = await service.ResetPasswordAsync(new ResetPasswordDto { Token = first, NewPassword = "Nueva-Clave1" });
        Assert.False(result.Success);
    }
}
