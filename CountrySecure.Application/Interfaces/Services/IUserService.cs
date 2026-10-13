using CountrySecure.Application.DTOs.Users;

namespace CountrySecure.Application.Interfaces.Services
{
    public interface IUserService{
        Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
        /// <summary>
        /// Actualiza un usuario. Un Admin puede editar cualquier usuario (incluido el rol);
        /// el resto solo puede editar sus propios datos no sensibles (sin Role ni Dni).
        /// </summary>
        Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
        // Task<bool> DeleteUserAsync(Guid id);
        Task<UserResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<UserResponseDto>> GetAllAsync(int page, int size, string? role = null);

        Task<UserResponseDto?> ToggleActiveAsync(Guid id);
    }
}