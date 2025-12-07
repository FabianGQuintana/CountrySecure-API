namespace CountrySecure.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success {get; set;}
        public string? Message {get; set;}


        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;

        public DateTime Expiration { get; set; }


        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}