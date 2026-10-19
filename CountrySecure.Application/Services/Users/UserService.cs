using CountrySecure.Application.DTOs.Common;
using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Mappers;
using CountrySecure.Domain.Entities;
using CountrySecure.Domain.Exceptions;

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

        public async Task<PagedResult<UserResponseDto>> GetPagedAsync(PageQuery paging, string? role, string? search, bool includeInactive)
        {
            var page = await _userRepository.GetPagedAsync(paging, role, search, includeInactive);
            return page.Map(u => u.ToDto());
        }

        public async Task<UserResponseDto> GetByIdAsync(Guid id)
        {
            // Admin puede consultar también usuarios dados de baja
            var user = _currentUser.IsAdmin
                ? await _userRepository.GetByIdIncludingDeletedAsync(id)
                : await _userRepository.GetByIdAsync(id);

            return (user ?? throw new KeyNotFoundException($"User with {id} not found")).ToDto();
        }

        public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"User with {id} not found");

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

            if (dto.Email != null && !string.Equals(dto.Email, user.Email, StringComparison.OrdinalIgnoreCase)
                && await _userRepository.EmailExistsAsync(dto.Email, id))
                throw new ConflictException("Email is already registered.");

            if (dto.Dni.HasValue && dto.Dni.Value != user.Dni && await _userRepository.DniExistsAsync(dto.Dni.Value, id))
                throw new ConflictException("DNI is already registered.");

            user.Name = dto.Name ?? user.Name;
            user.Lastname = dto.Lastname ?? user.Lastname;
            user.Dni = dto.Dni ?? user.Dni;
            user.Phone = dto.Phone ?? user.Phone;
            user.Email = dto.Email ?? user.Email;
            user.Role = dto.Role ?? user.Role;
            user.Touch(_currentUser.UserId?.ToString());

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }

        public async Task<UserResponseDto> DeactivateAsync(Guid id)
        {
            if (_currentUser.UserId == id)
                throw new BusinessRuleException("You cannot deactivate your own account.");

            var user = await _userRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"User with id {id} not found");

            user.SoftDelete(_currentUser.UserId?.ToString());

            // Un usuario dado de baja no puede seguir usando sus sesiones activas
            await _userRepository.RevokeAllRefreshTokensAsync(id);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }

        public async Task<UserResponseDto> SetBlockedAsync(Guid id, BlockDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"User with id {id} not found");

            user.IsBlocked = dto.Blocked;
            user.BlockReason = dto.Blocked ? dto.Reason?.Trim() : null;
            user.Touch(_currentUser.UserId?.ToString());

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }

        public async Task<UserResponseDto> RestoreAsync(Guid id)
        {
            var user = await _userRepository.GetByIdIncludingDeletedAsync(id)
                       ?? throw new KeyNotFoundException($"User with id {id} not found");

            user.Restore(_currentUser.UserId?.ToString());

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }
    }
}
