using System.Net;
using System.Security.Cryptography;
using System.Text;
using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Services.Auth
{
    /// <summary>
    /// Recuperación de contraseña por email con un enlace de un solo uso.
    /// Solo se guarda el hash del token; el token en claro viaja únicamente en el enlace.
    /// </summary>
    public class PasswordResetService : IPasswordResetService
    {
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(30);

        // Límite por cuenta (además del límite por IP del endpoint): evita llenar la casilla de alguien
        private const int MaxRequestsPerHour = 3;

        private const string InvalidLinkMessage = "El enlace de recuperación no es válido o ya venció. Pedí uno nuevo.";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;

        public PasswordResetService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
        }

        public async Task RequestResetAsync(ForgotPasswordDto dto, string resetPageUrl)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
            if (user == null) return;

            var since = DateTime.UtcNow.AddHours(-1);
            if (await _unitOfWork.Users.CountPasswordResetTokensSinceAsync(user.Id, since) >= MaxRequestsPerHour)
                return;

            // Un enlace nuevo invalida los anteriores
            await _unitOfWork.Users.InvalidatePasswordResetTokensAsync(user.Id);

            var token = NewToken();
            await _unitOfWork.Users.AddPasswordResetTokenAsync(new PasswordResetToken
            {
                TokenHash = Hash(token),
                UserId = user.Id,
                Expires = DateTime.UtcNow.Add(TokenLifetime)
            });
            await _unitOfWork.SaveChangesAsync();

            var link = $"{resetPageUrl.TrimEnd('/')}?token={Uri.EscapeDataString(token)}";
            var name = WebUtility.HtmlEncode(user.Name);
            var minutes = (int)TokenLifetime.TotalMinutes;

            await _emailSender.SendAsync(
                user.Email,
                "Recuperá tu contraseña de CountrySecure",
                $"<p>Hola {name}:</p>" +
                "<p>Recibimos un pedido para restablecer la contraseña de tu cuenta de CountrySecure.</p>" +
                $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Elegir una nueva contraseña</a></p>" +
                $"<p>El enlace vence en {minutes} minutos y sirve una sola vez. Si no lo pediste, ignorá este mensaje: tu contraseña no cambia.</p>",
                $"Hola {user.Name}:\n\nPara elegir una nueva contraseña de CountrySecure abrí este enlace:\n{link}\n\n" +
                $"Vence en {minutes} minutos y sirve una sola vez. Si no lo pediste, ignorá este mensaje.");
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var stored = await _unitOfWork.Users.GetPasswordResetTokenAsync(Hash(dto.Token));
            if (stored == null || stored.UsedAt != null || stored.Expires < DateTime.UtcNow)
                return (false, InvalidLinkMessage);

            var user = await _unitOfWork.Users.GetByIdAsync(stored.UserId);
            if (user == null)
                return (false, InvalidLinkMessage);

            user.Password = _passwordHasher.Hash(dto.NewPassword);
            user.Touch(user.Id.ToString());
            stored.UsedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            // Cualquier sesión abierta (posiblemente de quien conocía la contraseña vieja) se cierra
            await _unitOfWork.Users.RevokeAllRefreshTokensAsync(user.Id);
            await _unitOfWork.SaveChangesAsync();

            return (true, null);
        }

        private static string NewToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                      .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static string Hash(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
