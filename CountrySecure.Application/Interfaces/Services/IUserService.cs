using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Users;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Actualiza un usuario. Un Admin puede editar cualquier usuario (incluido el rol);
        /// el resto solo puede editar sus propios datos no sensibles (sin Role ni Dni).
        /// </summary>
        Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto);

        /// <summary>Da de baja al usuario y revoca sus sesiones. Un Admin no puede darse de baja a sí mismo.</summary>
        Task<UserResponseDto> DeactivateAsync(Guid id);
        Task<UserResponseDto> RestoreAsync(Guid id);

        /// <summary>Un residente bloqueado no puede recibir visitas: sus pases se rechazan en la guardia.</summary>
        Task<UserResponseDto> SetBlockedAsync(Guid id, BlockDto dto);

        /// <summary>Cada usuario elige su propio avatar entre los predefinidos (null = iniciales).</summary>
        Task<UserResponseDto> UpdateAvatarAsync(Guid userId, string? avatar);

        Task<UserResponseDto> GetByIdAsync(Guid id);
        Task<PagedResult<UserResponseDto>> GetPagedAsync(PageQuery paging, string? role, string? search, bool includeInactive);
    }
}
