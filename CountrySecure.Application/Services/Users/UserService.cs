using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Services.Users
{
    public class UserService : IUserService
    {

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }


        public async Task<IEnumerable<UserResponseDto>> GetAllAsync(int page, int size, string? role = null)
        {
            var users = await _userRepository.GetAllAsync(page, size, role);

            return users.Select(u => u.ToDto());
        }

        public async Task<UserResponseDto> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null) throw new KeyNotFoundException($"User with {id} not found");

            return user.ToDto();
        }

        public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null) throw new KeyNotFoundException($"User with {id} not found");

            if (!_currentUser.IsAdmin)
            {
                // Un usuario que no es Admin solo puede editarse a sí mismo...
                if (_currentUser.UserId != id)
                    throw new UnauthorizedAccessException("You can only update your own profile.");

                // ...y nunca puede cambiar campos sensibles.
                if (dto.Role != null && dto.Role != user.Role)
                    throw new UnauthorizedAccessException("Only an Admin can change a user's role.");

                if (dto.Dni.HasValue && dto.Dni.Value != user.Dni)
                    throw new UnauthorizedAccessException("Only an Admin can change a user's DNI.");
            }

            user.Name = dto.Name ?? user.Name;
            user.Lastname = dto.Lastname ?? user.Lastname;
            user.Dni = dto.Dni ?? user.Dni;
            user.Phone = dto.Phone ?? user.Phone;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }

        public async Task<UserResponseDto?> ToggleActiveAsync(Guid id)
        {
            var user = await _userRepository.ToggleActiveAsync(id);
            if (user == null) return null;

            await _unitOfWork.SaveChangesAsync();

            // Simplemente devolver el DTO
            return user.ToDto();
        }
    }
}