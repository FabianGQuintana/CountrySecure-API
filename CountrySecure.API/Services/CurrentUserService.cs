using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Constants;

namespace CountrySecure.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId
        {
            get
            {
                // El token emite el id en "id", "sub" y (mapeado por JwtBearer) NameIdentifier
                var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                            ?? Principal?.FindFirstValue("id");

                return Guid.TryParse(value, out var id) ? id : null;
            }
        }

        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

        public bool IsAdmin => Role == RoleTypes.Admin;

        public Guid GetRequiredUserId()
            => UserId ?? throw new UnauthorizedAccessException("User ID not found or invalid in token.");
    }
}
