namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>Envío de emails. Sin SMTP configurado, la implementación por defecto solo los registra en el log.</summary>
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken cancellationToken = default);
    }
}
