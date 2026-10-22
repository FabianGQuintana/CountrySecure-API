using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace CountrySecure.Infrastructure.Services
{
    /// <summary>
    /// Token del QR: <c>{id}.{firma}</c>, donde la firma es un HMAC-SHA256 del id con una clave que solo conoce el servidor.
    /// Quien escanee o fotografíe el QR no puede fabricar el token de otro permiso ni alterar el id.
    /// </summary>
    public class PassTokenService : IPassTokenService
    {
        // 16 bytes (128 bits) de firma: suficiente para impedir falsificaciones y mantiene el QR compacto
        private const int SignatureBytes = 16;

        private readonly byte[] _key;

        public PassTokenService(IConfiguration configuration)
        {
            var secret = configuration["Pass:SigningKey"] ?? configuration["Jwt:Key"]
                         ?? throw new InvalidOperationException(
                             "Falta 'Pass:SigningKey' (o 'Jwt:Key') para firmar los pases.");

            // Separación de dominios: la clave de los pases deriva de la secreta pero no es la del JWT
            using var derive = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            _key = derive.ComputeHash(Encoding.UTF8.GetBytes("country-secure/pass-token/v1"));
        }

        public string Create(Guid permissionId)
            => $"{permissionId:N}.{Base64Url.EncodeToString(Sign(permissionId))}";

        public bool TryValidate(string token, out Guid permissionId)
        {
            permissionId = default;

            var parts = token.Split('.');
            if (parts.Length != 2 || !Guid.TryParseExact(parts[0], "N", out var id))
                return false;

            byte[] given;
            try
            {
                given = Base64Url.DecodeFromChars(parts[1]);
            }
            catch (FormatException)
            {
                return false;
            }

            // Comparación en tiempo constante: no filtra información por diferencias de tiempo
            if (!CryptographicOperations.FixedTimeEquals(given, Sign(id)))
                return false;

            permissionId = id;
            return true;
        }

        private byte[] Sign(Guid id)
        {
            using var hmac = new HMACSHA256(_key);
            return hmac.ComputeHash(id.ToByteArray())[..SignatureBytes];
        }
    }
}
