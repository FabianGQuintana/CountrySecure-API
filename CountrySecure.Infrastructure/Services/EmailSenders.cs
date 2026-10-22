using System.Net;
using System.Net.Mail;
using CountrySecure.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CountrySecure.Infrastructure.Services
{
    /// <summary>
    /// Envío por SMTP. Se usa cuando está configurada la sección <c>Smtp</c>
    /// (Host, Port, User, Password, From, EnableSsl).
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken cancellationToken = default)
        {
            var smtp = _configuration.GetSection("Smtp");
            using var client = new SmtpClient(smtp["Host"], int.TryParse(smtp["Port"], out var port) ? port : 587)
            {
                EnableSsl = !bool.TryParse(smtp["EnableSsl"], out var ssl) || ssl,
                Credentials = string.IsNullOrEmpty(smtp["User"]) ? null : new NetworkCredential(smtp["User"], smtp["Password"])
            };

            using var message = new MailMessage(smtp["From"] ?? smtp["User"]!, to, subject, htmlBody) { IsBodyHtml = true };
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));

            try
            {
                await client.SendMailAsync(message, cancellationToken);
            }
            catch (SmtpException ex)
            {
                // No se propaga: la respuesta al usuario no debe revelar si la cuenta existe
                _logger.LogError(ex, "No se pudo enviar el email \"{Subject}\" a {To}", subject, to);
            }
        }
    }

    /// <summary>Sin SMTP configurado: en desarrollo el email (con el enlace) se escribe en el log de la API.</summary>
    public class LogEmailSender : IEmailSender
    {
        private readonly ILogger<LogEmailSender> _logger;
        private readonly bool _includeBody;

        /// <param name="includeBody">Solo en desarrollo: el cuerpo trae el enlace de recuperación.</param>
        public LogEmailSender(ILogger<LogEmailSender> logger, bool includeBody)
        {
            _logger = logger;
            _includeBody = includeBody;
        }

        public Task SendAsync(string to, string subject, string htmlBody, string textBody, CancellationToken cancellationToken = default)
        {
            if (_includeBody)
                _logger.LogWarning("Email no enviado (falta configurar Smtp). Para: {To} · Asunto: {Subject}\n{Body}", to, subject, textBody);
            else
                _logger.LogError("Email no enviado: falta configurar la sección Smtp. Para: {To} · Asunto: {Subject}", to, subject);
            return Task.CompletedTask;
        }
    }
}
