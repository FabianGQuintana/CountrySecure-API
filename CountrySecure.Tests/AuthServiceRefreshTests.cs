using System.Security.Claims;
using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Services.Auth;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Exceptions;
using CountrySecure.Infrastructure.Persistence;
using NSubstitute;

namespace CountrySecure.Tests;

/// <summary>Rotación del refresh token: un solo uso, con ventana de gracia de 30 s para requests simultáneas.</summary>
public class AuthServiceRefreshTests
{
    private static (AuthService service, ITokenService tokens) Build(CountrySecureDbContext db)
    {
        var tokens = Substitute.For<ITokenService>();
        var counter = 0;
        tokens.GenerateToken(Arg.Any<User>()).Returns(_ => $"access-{++counter}");
        tokens.GenerateRefreshToken().Returns(_ => $"refresh-{++counter}");

        var service = new AuthService(TestSupport.NewUnitOfWork(db), tokens, Substitute.For<IPasswordHasher>());
        return (service, tokens);
    }

    private static void TokenBelongsTo(ITokenService tokens, Guid userId)
        => tokens.GetPrincipalFromExpiredToken(Arg.Any<string>())
            .Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim("id", userId.ToString())])));

    private static async Task<RefreshToken> AddRefreshTokenAsync(CountrySecureDbContext db, Guid userId, Action<RefreshToken>? tweak = null)
    {
        var token = new RefreshToken { Token = $"seed-{Guid.NewGuid():N}", UserId = userId, Expires = DateTime.UtcNow.AddDays(7) };
        tweak?.Invoke(token);
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();
        return token;
    }

    private static RefreshTokenRequest Request(string refresh) => new() { AccessToken = "expired-access", RefreshToken = refresh };

    [Fact]
    public async Task Rota_el_token_y_marca_el_anterior_como_usado()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var stored = await AddRefreshTokenAsync(db, user.Id);
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        var result = await service.RefreshTokenAsync(Request(stored.Token));

        Assert.NotEqual(stored.Token, result.RefreshToken);
        Assert.True(stored.IsUsed);
        Assert.Equal(result.RefreshToken, stored.ReplacedByToken);
        Assert.Single(db.RefreshTokens.Where(t => t.Token == result.RefreshToken));
    }

    [Fact]
    public async Task Dentro_de_la_ventana_de_gracia_devuelve_el_mismo_reemplazo()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var stored = await AddRefreshTokenAsync(db, user.Id);
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        var first = await service.RefreshTokenAsync(Request(stored.Token));
        var second = await service.RefreshTokenAsync(Request(stored.Token));

        Assert.Equal(first.RefreshToken, second.RefreshToken);
    }

    [Fact]
    public async Task Fuera_de_la_ventana_de_gracia_el_token_usado_se_rechaza()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var replacement = await AddRefreshTokenAsync(db, user.Id);
        var used = await AddRefreshTokenAsync(db, user.Id, t =>
        {
            t.IsUsed = true;
            t.UsedAt = DateTime.UtcNow.AddSeconds(-31);
            t.ReplacedByToken = replacement.Token;
        });
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.RefreshTokenAsync(Request(used.Token)));
    }

    [Fact]
    public async Task Un_token_revocado_no_sirve()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var revoked = await AddRefreshTokenAsync(db, user.Id, t => t.IsRevoked = true);
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.RefreshTokenAsync(Request(revoked.Token)));
    }

    [Fact]
    public async Task Un_token_vencido_no_sirve()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var expired = await AddRefreshTokenAsync(db, user.Id, t => t.Expires = DateTime.UtcNow.AddMinutes(-1));
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.RefreshTokenAsync(Request(expired.Token)));
    }

    [Fact]
    public async Task El_refresh_token_debe_pertenecer_al_dueno_del_access_token()
    {
        using var db = TestSupport.NewDb();
        var owner = await TestSupport.AddUserAsync(db);
        var attacker = await TestSupport.AddUserAsync(db);
        var stored = await AddRefreshTokenAsync(db, owner.Id);
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, attacker.Id);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.RefreshTokenAsync(Request(stored.Token)));
    }

    [Fact]
    public async Task Un_usuario_dado_de_baja_no_puede_renovar_la_sesion()
    {
        using var db = TestSupport.NewDb();
        var user = await TestSupport.AddUserAsync(db);
        var stored = await AddRefreshTokenAsync(db, user.Id);
        user.SoftDelete();
        await db.SaveChangesAsync();
        var (service, tokens) = Build(db);
        TokenBelongsTo(tokens, user.Id);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.RefreshTokenAsync(Request(stored.Token)));
    }
}
