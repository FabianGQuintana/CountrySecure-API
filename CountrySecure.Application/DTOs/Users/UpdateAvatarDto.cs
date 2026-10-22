namespace CountrySecure.Application.DTOs.Users
{
    public class UpdateAvatarDto
    {
        /// <summary>Clave del avatar (avatar-1 … avatar-5) o null para volver a las iniciales.</summary>
        public string? Avatar { get; set; }
    }
}
