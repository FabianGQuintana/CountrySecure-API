namespace CountrySecure.Tests;

/// <summary>El QR solo lleva un token firmado: no se puede falsificar ni alterar.</summary>
public class PassTokenServiceTests
{
    [Fact]
    public void Un_token_emitido_se_valida_y_devuelve_el_mismo_id()
    {
        var tokens = TestSupport.Tokens();
        var id = Guid.NewGuid();

        var token = tokens.Create(id);

        Assert.True(tokens.TryValidate(token, out var parsed));
        Assert.Equal(id, parsed);
        Assert.DoesNotContain(id.ToString(), token); // el id va sin guiones, nunca el texto plano estándar
    }

    [Fact]
    public void Cambiar_el_id_invalida_la_firma()
    {
        var tokens = TestSupport.Tokens();
        var token = tokens.Create(Guid.NewGuid());
        var signature = token[(token.IndexOf('.') + 1)..];

        var forged = $"{Guid.NewGuid():N}.{signature}";

        Assert.False(tokens.TryValidate(forged, out _));
    }

    [Fact]
    public void Alterar_la_firma_invalida_el_token()
    {
        var tokens = TestSupport.Tokens();
        var token = tokens.Create(Guid.NewGuid());

        var last = token[^1] == 'A' ? 'B' : 'A';
        var tampered = token[..^1] + last;

        Assert.False(tokens.TryValidate(tampered, out _));
    }

    [Fact]
    public void Un_token_firmado_con_otra_clave_no_es_valido()
    {
        var token = TestSupport.Tokens("otra-clave-distinta-0123456789-abcdefghij").Create(Guid.NewGuid());

        Assert.False(TestSupport.Tokens().TryValidate(token, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("basura")]
    [InlineData("abc.def")]
    [InlineData("....")]
    [InlineData("00000000000000000000000000000000.")]
    public void Los_tokens_mal_formados_se_rechazan_sin_lanzar_excepciones(string token)
    {
        Assert.False(TestSupport.Tokens().TryValidate(token, out _));
    }
}
