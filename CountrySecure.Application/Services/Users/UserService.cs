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

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }


        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {

            var user = new User
            {
                Name = dto.Name,
                Lastname = dto.Lastname,
                Dni = dto.Dni,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = dto.Password,
                Role = dto.Role,
                Status = "Active"
                
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user.ToDto();
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync(int page, int size)
        {
            var users = await _userRepository.GetAllAsync(page, size);

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

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var deleted = await _userRepository.DeleteAsync(id);

            if (!deleted) return false;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}