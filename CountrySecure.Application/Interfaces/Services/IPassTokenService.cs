namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>
    /// Tokens firmados para los QR de los pases. El QR nunca contiene datos en claro:
    /// solo el id del permiso y una firma HMAC que no se puede fabricar sin la clave del servidor.
    /// </summary>
    public interface IPassTokenService
    {
        string Create(Guid permissionId);

        /// <summary>Verifica la firma (comparación en tiempo constante) y devuelve el id del permiso.</summary>
        bool TryValidate(string token, out Guid permissionId);
    }
}
