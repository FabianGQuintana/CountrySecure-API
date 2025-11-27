using CountrySecure.Application.DTOs.Users;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Application.Interfaces.Users;

namespace CountrySecure.Application.Services.Users
{
    public class UserService(IUserRepository userRepository) : IUserService
    {

        private readonly IUserRepository _userRepository = userRepository;

        public Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<UserResponseDto>> GetAllAsync(int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponseDto> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}