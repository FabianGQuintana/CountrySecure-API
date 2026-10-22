using CountrySecure.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CountrySecure.API.Middleware
{
    /// <summary>
    /// Traduce las excepciones no controladas a respuestas RFC 7807 (ProblemDetails).
    /// Reemplaza los try/catch que antes se repetían en cada controlador.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "No se encontró el recurso."),
                AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "No se pudo validar la sesión."),
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto con los datos existentes."),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "La operación no cumple una regla del sistema."),
                SecurityTokenException => (StatusCodes.Status401Unauthorized, "La sesión no es válida."),
                UnauthorizedAccessException when httpContext.User.Identity?.IsAuthenticated != true
                    => (StatusCodes.Status401Unauthorized, "Tenés que iniciar sesión."),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "No tenés permiso para esta acción."),
                ArgumentException => (StatusCodes.Status400BadRequest, "La solicitud no es válida."),
                InvalidOperationException => (StatusCodes.Status409Conflict, "La operación no está permitida."),
                DbUpdateException => (StatusCodes.Status409Conflict, "Los datos entran en conflicto con un registro existente."),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado. Intentá de nuevo.")
            };

            if (status >= 500)
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            else
                _logger.LogWarning("Handled {Exception} on {Method} {Path}: {Message}", exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

            // Los mensajes de negocio (4xx) son seguros para el cliente; los 5xx y los errores de BD no se exponen.
            var detail = status >= 500 || exception is DbUpdateException
                ? (_environment.IsDevelopment() ? exception.ToString() : "Ocurrió un error inesperado.")
                : exception.Message;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            // Compatibilidad con el frontend actual, que lee `message`
            problem.Extensions["message"] = status >= 500 || exception is DbUpdateException ? title : detail;

            httpContext.Response.StatusCode = status;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
