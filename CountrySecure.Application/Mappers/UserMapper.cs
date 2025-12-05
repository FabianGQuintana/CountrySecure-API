using CountrySecure.Application.DTOs.Users;
using CountrySecure.Domain.Entities;
using CountrySecure.Application.DTOs.EntryPermission;

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

        // Nuevo: Mapeo para el DTO Anidado (EntryPermissionUserDto)
        public static EntryPermissionUserDto ToEntryPermissionUserDto(this User user)
        {
            return new EntryPermissionUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Dni = user.Dni,
                Phone = user.Phone,
                Email = user.Email
                // Excluimos datos sensibles como Password, Active y Role.
            };
        }
    }
}