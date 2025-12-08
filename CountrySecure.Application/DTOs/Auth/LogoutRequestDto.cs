namespace CountrySecure.Application.DTOs.Auth
{
    public class LogoutRequestDto
    {
        public required string RefreshToken {get; set;}
    }
}