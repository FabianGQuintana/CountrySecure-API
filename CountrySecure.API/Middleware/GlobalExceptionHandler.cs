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
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
                SecurityTokenException => (StatusCodes.Status401Unauthorized, "Invalid token"),
                UnauthorizedAccessException when httpContext.User.Identity?.IsAuthenticated != true
                    => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
                InvalidOperationException => (StatusCodes.Status409Conflict, "Operation not allowed"),
                DbUpdateException => (StatusCodes.Status409Conflict, "Data conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            if (status >= 500)
                _logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            else
                _logger.LogWarning("Handled {Exception} on {Method} {Path}: {Message}", exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

            // Los mensajes de negocio (4xx) son seguros para el cliente; los 5xx y los errores de BD no se exponen.
            var detail = status >= 500 || exception is DbUpdateException
                ? (_environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.")
                : exception.Message;

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
            // Compatibilidad con el frontend actual, que lee `message`
            problem.Extensions["message"] = status >= 500 ? title : detail;

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
