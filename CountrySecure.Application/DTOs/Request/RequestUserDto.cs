namespace CountrySecure.Application.DTOs.Request
{
    public class RequestUserDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; } 
        public required string Lastname { get; set; } 
        public required string Email { get; set; } 
    }
}
