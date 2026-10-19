using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.EntryPermission;
using CountrySecure.Domain.Enums;

namespace CountrySecure.Application.Interfaces.Services
{
    /// <summary>Control de acceso: escaneo de pases en la guardia, pases digitales y auditoría.</summary>
    public interface IAccessControlService
    {
        /// <summary>Valida el QR y, si corresponde, registra el ingreso o la salida. Siempre deja un registro de auditoría.</summary>
        Task<ScanResultDto> ScanAsync(ScanRequestDto dto, Guid guardId);

        /// <summary>Pase de un permiso: su titular, Admin o Seguridad.</summary>
        Task<PassDto> GetPassAsync(Guid permissionId);

        /// <summary>Pase por token firmado (enlace público para la visita). Lanza KeyNotFoundException si no es válido.</summary>
        Task<PassDto> GetPublicPassAsync(string token);

        Task<PagedResult<AccessLogResponseDto>> GetLogsAsync(PageQuery paging, Guid? permissionId, ScanOutcome? outcome, DateTime? from, DateTime? to);
    }
}
