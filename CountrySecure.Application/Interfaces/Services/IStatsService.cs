using CountrySecure.Application.DTOs.Stats;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IStatsService
    {
        Task<AdminStatsDto> GetAdminStatsAsync();
        Task<ResidentStatsDto> GetResidentStatsAsync(Guid residentId);
        Task<SecurityStatsDto> GetSecurityStatsAsync();
    }
}
