using CountrySecure.Application.DTOs.Users;
using CountrySecure.Domain.Entities;

namespace CountrySecure.Application.Mappers
{
    public static class UserMapper
    {
        public static UserResponseDto ToDto(this User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Dni = user.Dni,
                Phone = user.Phone,
                Email = user.Email,
                Active = user.Active,
                Role = user.Role.ToString()
            };
        }
    }
}