

using CountrySecure.Application.DTOs.Auth;
using CountrySecure.Application.Interfaces.Persistence;
using CountrySecure.Application.Interfaces.Repositories;
using CountrySecure.Application.Interfaces.Services;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Infrastructure.Services
{
    public class AuthService : IAuthService
    {

        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService
        )
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already registered"
                };
            }
                

            var user = new User
            {
                Name = dto.Name,
                Lastname = dto.Lastname,
                Dni = dto.Dni,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(8),
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role
            };

        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null) {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }
                

            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(8),
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role
            };
        }


    }
}