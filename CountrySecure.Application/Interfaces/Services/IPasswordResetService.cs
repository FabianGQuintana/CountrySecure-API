using CountrySecure.Application.DTOs.Auth;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IPasswordResetService
    {
        /// <summary>
        /// Si el email pertenece a un usuario activo, genera un enlace de un solo uso y lo envía.
        /// No informa si la cuenta existe (no permite enumerar usuarios).
        /// </summary>
        Task RequestResetAsync(ForgotPasswordDto dto, string resetPageUrl);

        /// <summary>Cambia la contraseña con un enlace vigente y cierra todas las sesiones abiertas.</summary>
        Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
